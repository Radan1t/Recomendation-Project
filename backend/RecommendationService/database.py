import os
import pandas as pd
from sqlalchemy import create_engine

db_user = os.environ.get("POSTGRES_USER", "admin")
db_pass = os.environ.get("POSTGRES_PASSWORD", "password123")
db_host = os.environ.get("POSTGRES_HOST", "postgres")
db_port = os.environ.get("POSTGRES_PORT", "5432")

USER_DB_URL = os.environ.get(
    "USER_DB_URL", 
    f"postgresql://{db_user}:{db_pass}@{db_host}:{db_port}/UserDb"
)
CONTENT_DB_URL = os.environ.get(
    "CONTENT_DB_URL", 
    f"postgresql://{db_user}:{db_pass}@{db_host}:{db_port}/ContentDb"
)
INTERACT_DB_URL = os.environ.get(
    "INTERACT_DB_URL", 
    f"postgresql://{db_user}:{db_pass}@{db_host}:{db_port}/InteractionDb"
)

engine_user = create_engine(USER_DB_URL)
engine_content = create_engine(CONTENT_DB_URL)
engine_interact = create_engine(INTERACT_DB_URL)

def fetch_from_user(query: str, params=None):
    with engine_user.connect() as conn:
        return pd.read_sql(query, conn, params=params)

def fetch_from_content(query: str, params=None):
    with engine_content.connect() as conn:
        return pd.read_sql(query, conn, params=params)

def fetch_from_interact(query: str, params=None):
    with engine_interact.connect() as conn:
        return pd.read_sql(query, conn, params=params)