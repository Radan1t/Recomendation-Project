from fastapi import FastAPI, HTTPException
from sqlalchemy import text
from datetime import datetime
from recommender import HybridRecommender
from database import engine_interact 

app = FastAPI(title="Recommendation Engine Service")
recommender = HybridRecommender()

@app.post("/api/v1/recommendations/generate/{user_id}")
async def generate_recommendations(user_id: int):
    try:

        recs = recommender.generate_hybrid(user_id, total_recommendations=100, per_type_min=12)
        
        if not recs:
            raise HTTPException(status_code=404, detail="No content found")

        with engine_interact.begin() as conn:


            conn.execute(text("""
                DELETE FROM "RecommendedContents" 
                WHERE "SessionID" IN (SELECT "SessionID" FROM "RecommendationSessions" WHERE "UserID" = :uid)
            """), {"uid": user_id})
            
            conn.execute(text("""
                DELETE FROM "RecommendationSessions" WHERE "UserID" = :uid
            """), {"uid": user_id})

            result = conn.execute(text("""
                INSERT INTO "RecommendationSessions" ("DateGenerated", "AlgorithmType", "UserID")
                VALUES (:date, 'Hybrid_Dynamic_V2', :uid)
                RETURNING "SessionID"
            """), {
                "date": datetime.utcnow(),
                "uid": user_id
            })
            session_id = result.fetchone()[0]

            insert_data = [
                {
                    "cid": item['ContentID'],
                    "sid": session_id,
                    "rank": i,
                    "score": float(item['hybrid_score'])
                }
                for i, item in enumerate(recs, start=1)
            ]
            
            conn.execute(text("""
                INSERT INTO "RecommendedContents" ("ContentID", "SessionID", "RankPosition", "RelevanceScore")
                VALUES (:cid, :sid, :rank, :score)
            """), insert_data)

        return {
            "session_id": session_id,
            "user_id": user_id,
            "recommendations": recs
        }

    except Exception as e:
        print(f"CRITICAL ERROR: {e}")
        raise HTTPException(status_code=500, detail=str(e))



@app.get("/api/v1/recommendations/{content_id}")
async def get_similar_content_endpoint(content_id: int):
    try:

        recs = recommender.get_similar_content(content_id, top_n=5)
        
        return recs
    except Exception as e:
        print(f"CRITICAL ERROR: {e}")
        raise HTTPException(status_code=500, detail=str(e))
