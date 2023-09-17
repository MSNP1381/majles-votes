import sqlite3 ,pandas as pd

conn = sqlite3.connect('data.db', isolation_level=None,
                       detect_types=sqlite3.PARSE_COLNAMES)
with open("export.sql") as f:
    db_df = pd.read_sql_query(f.read(), conn)

db_df.to_csv('votes.csv', index=False)

