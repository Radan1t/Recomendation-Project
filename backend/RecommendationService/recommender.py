import pandas as pd
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
from database import fetch_from_content, fetch_from_user, fetch_from_interact

class HybridRecommender:
    def __init__(self):
        self.df_contents = pd.DataFrame()
        self.tfidf_matrix = None
        self.vectorizer = TfidfVectorizer(stop_words='english')

    def load_data(self):
        """Універсальне завантаження: Games + Films + Books + Series"""

        query_content = """
            SELECT c."ContentID", c."Title", COALESCE(c."AverageRating", 0) as "AverageRating", c."PosterURL",
                   COALESCE(g."Developer", f."Director", b."Author", s."Network", 'Unknown') as "Creator",
                   COALESCE(string_agg(DISTINCT gn."Name", ' '), '') as genres,
                   COALESCE(string_agg(DISTINCT t."Name", ' '), '') as tags
            FROM "Contents" c
            LEFT JOIN "Games" g ON c."ContentID" = g."ContentID"
            LEFT JOIN "Films" f ON c."ContentID" = f."ContentID"
            LEFT JOIN "Books" b ON c."ContentID" = b."ContentID"
            LEFT JOIN "Series" s ON c."ContentID" = s."ContentID"
            LEFT JOIN "ContentGenres" cg ON c."ContentID" = cg."ContentID"
            LEFT JOIN "Genres" gn ON cg."GenreID" = gn."GenreID"
            LEFT JOIN "ContentTags" ct ON c."ContentID" = ct."ContentID"
            LEFT JOIN "Tags" t ON ct."TagID" = t."TagID"
            GROUP BY c."ContentID", c."Title", c."AverageRating", c."PosterURL",
                     g."Developer", f."Director", b."Author", s."Network"
        """
        self.df_contents = fetch_from_content(query_content)

        self.df_contents['features'] = (
            self.df_contents['genres'] + " " + 
            self.df_contents['tags'] + " " + 
            self.df_contents['Creator']
        ).fillna('')
        
        self.tfidf_matrix = self.vectorizer.fit_transform(self.df_contents['features'])

    def get_dynamic_user_profile(self, user_id: int):
        """Збирає інтереси з профілю, обраного та високих оцінок"""

        q_profile = """
            SELECT COALESCE(string_agg(DISTINCT g."Name", ' '), '') as g,
                   COALESCE(string_agg(DISTINCT t."Name", ' '), '') as t
            FROM "UserProfiles" up
            LEFT JOIN "ProfileGenres" pg ON up."UserID" = pg."UserID"
            LEFT JOIN "Genres" g ON pg."GenreID" = g."GenreID"
            LEFT JOIN "ProfileTags" pt ON up."UserID" = pt."UserID"
            LEFT JOIN "Tags" t ON pt."TagID" = t."TagID"
            WHERE up."UserID" = %(uid)s
        """
        df_p = fetch_from_user(q_profile, {"uid": user_id})
        static_f = (df_p['g'][0] + " " + df_p['t'][0]).strip() if not df_p.empty else ""

        q_inter = """
            SELECT "ContentID" FROM "UserFavorites" WHERE "UserID" = %(uid)s
            UNION ALL
            SELECT "ContentID" FROM "UserRatings" WHERE "UserID" = %(uid)s AND "Score" >= 4
        """
        liked_ids = fetch_from_interact(q_inter, {"uid": user_id})['ContentID'].tolist()
        
        dynamic_f = ""
        if liked_ids:

            features = self.df_contents[self.df_contents['ContentID'].isin(liked_ids)]['features'].tolist()
            dynamic_f = " ".join(features)

        return (static_f + " " + dynamic_f).strip()

    def get_content_based_scores(self, user_id: int):
        profile_text = self.get_dynamic_user_profile(user_id)
        if not profile_text: return np.zeros(len(self.df_contents))

        user_vector = self.vectorizer.transform([profile_text])
        return cosine_similarity(user_vector, self.tfidf_matrix).flatten()

    def get_collaborative_scores(self, user_id: int):

        q_ratings = """
            SELECT "UserID", "ContentID", "Score" FROM "UserRatings"
            UNION ALL
            SELECT "UserID", "ContentID", 5 as "Score" FROM "UserFavorites"
        """
        df_r = fetch_from_interact(q_ratings)
        if df_r.empty or user_id not in df_r['UserID'].values:
            return self.df_contents['AverageRating'].fillna(0).values / 10.0

        pivot = df_r.pivot_table(index='UserID', columns='ContentID', values='Score').fillna(0)
        item_sim = cosine_similarity(pivot.T)
        item_sim_df = pd.DataFrame(item_sim, index=pivot.columns, columns=pivot.columns)

        u_ratings = pivot.loc[user_id]
        scores = item_sim_df.dot(u_ratings) / (item_sim_df.sum(axis=1) + 1e-9)
        
        cf_res = np.zeros(len(self.df_contents))
        for i, row in self.df_contents.iterrows():
            cid = row['ContentID']
            if cid in scores.index: cf_res[i] = scores[cid]
        return cf_res / cf_res.max() if cf_res.max() > 0 else cf_res

    def generate_hybrid(self, user_id: int, top_n: int = 200):
        self.load_data()
        cbf = self.get_content_based_scores(user_id)
        cf = self.get_collaborative_scores(user_id)

        hybrid_scores = (0.6 * cbf) + (0.4 * cf)
        
        df_res = self.df_contents.copy()
        df_res['hybrid_score'] = hybrid_scores

        df_res['hybrid_score'] *= np.random.uniform(0.9, 1.1, size=len(df_res))

        q_seen = """
            SELECT "ContentID" FROM "UserRatings" WHERE "UserID" = %(uid)s
            UNION
            SELECT "ContentID" FROM "UserFavorites" WHERE "UserID" = %(uid)s
        """
        seen_ids = fetch_from_interact(q_seen, {"uid": user_id})['ContentID'].tolist()
        df_res = df_res[~df_res['ContentID'].isin(seen_ids)]
        
        return df_res.sort_values(by='hybrid_score', ascending=False).head(top_n)\
               [['ContentID', 'Title', 'hybrid_score']].to_dict(orient='records')



    def get_similar_content(self, content_id: int, top_n: int = 5):
        """Повертає схожий контент на основі TF-IDF матриці"""
        if self.df_contents.empty:
            self.load_data()

        idx_list = self.df_contents.index[self.df_contents['ContentID'] == content_id].tolist()
        if not idx_list:
            return []
            
        idx = idx_list[0]

        cosine_sim = cosine_similarity(self.tfidf_matrix[idx], self.tfidf_matrix).flatten()

        similar_indices = cosine_sim.argsort()[-(top_n + 1):-1][::-1]

        results = []
        for i in similar_indices:
            row = self.df_contents.iloc[i]
            results.append({
                "id": int(row['ContentID']),
                "title": str(row['Title']),
                "posterUrl": str(row['PosterURL']) if pd.notna(row['PosterURL']) else None
            })
            
        return results
