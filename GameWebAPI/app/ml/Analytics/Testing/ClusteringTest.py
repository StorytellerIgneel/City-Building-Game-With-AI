import pandas as pd
import joblib
import os

from app.ml.Analytics.preprocessing.Preprocess_TurnSnapshots import preprocess_turn_snapshot_regression

# ===== PATHS =====
CSV_PATH = r"C:\UnityProjects\FYP\GameWebAPI\app\ml\Analytics\Data\TurnSnapShots\turn_snapshots_20260406_022221.csv"

MODEL_DIR = r"C:\UnityProjects\FYP\GameWebAPI\app\ml\TrainedModels\Regression"

lr_model = joblib.load(os.path.join(MODEL_DIR, "lr_model.pkl"))
lr_scaler = joblib.load(os.path.join(MODEL_DIR, "lr_scaler.pkl"))
lr_feature_cols = joblib.load(os.path.join(MODEL_DIR, "lr_feature_cols.pkl"))

# ===== LOAD CSV =====
df = pd.read_csv(CSV_PATH)

print("Original Data:")
print(df.head())

# ===== FILTER TURNS ≤ 16 =====
turn_col = "Turn" if "Turn" in df.columns else "turn"
df = df[df[turn_col] <= 16].copy()

print("\nFiltered Data (≤16):")
print(df.tail())

# ===== PREPROCESS =====
features = preprocess_turn_snapshot_regression(df)

if features is None:
    print("Not enough data for preprocessing")
    exit()

# ===== BUILD MODEL INPUT =====
X = pd.DataFrame([features])[lr_feature_cols]

print("\nFEATURES USED:")
print(X.T)

# ===== SCALE + PREDICT =====
X_scaled = lr_scaler.transform(X)
predicted_population = lr_model.predict(X_scaled)[0]

print("\nPredicted Final Population:", predicted_population)