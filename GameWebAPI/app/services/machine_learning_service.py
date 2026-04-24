from datetime import datetime, timezone
from uuid import uuid4
from typing import Dict, Any
from fastapi import APIRouter, HTTPException
import pandas as pd
import joblib, os

from app.models.dto import ActionLogDto, TurnSnapshotDto
from app.ml.Analytics.preprocessing.Preprocess_TurnSnapshots import preprocess_turn_snapshot_cluster, preprocess_turn_snapshot_regression

CLUSTER_LABEL_MAP = {
    0: "Planner",
    1: "Tempo",
    2: "Expander"
}

class MachineLearningService:
    def __init__(self, gemini_service, session_service):
        self.gemini = gemini_service
        self.session_service = session_service

        CLUSTERING_MODEL_DIR = r"C:\UnityProjects\FYP\GameWebAPI\app\ml\TrainedModels\Clustering"
        REGRESSION_MODEL_DIR = r"C:\UnityProjects\FYP\GameWebAPI\app\ml\TrainedModels\Regression"

        # -------- KMEANS --------
        self.kmeans_model = joblib.load(os.path.join(CLUSTERING_MODEL_DIR, "kmeans_model.pkl"))
        self.kmeans_scaler = joblib.load(os.path.join(CLUSTERING_MODEL_DIR, "scaler.pkl"))
        self.kmeans_feature_cols = joblib.load(os.path.join(CLUSTERING_MODEL_DIR, "feature_cols.pkl"))

        # -------- REGRESSION --------
        self.lr_model = joblib.load(os.path.join(REGRESSION_MODEL_DIR, "lr_model.pkl"))
        self.lr_scaler = joblib.load(os.path.join(REGRESSION_MODEL_DIR, "lr_scaler.pkl"))
        self.lr_feature_cols = joblib.load(os.path.join(REGRESSION_MODEL_DIR, "lr_feature_cols.pkl"))

    def predict_all(self, session_id: str):

        import pandas as pd

        df = pd.read_csv(r"c:\UnityProjects\FYP\GameWebAPI\app\ml\Analytics\Preprocessed\session_features.csv")
        avg_final_population = df["final_population"].mean()

        print(avg_final_population)

        session = self.session_service.get_session(session_id)
        if not session:
            raise HTTPException(status_code=404, detail="Session not found.")

        turn_snapshots = session.get("turn_snapshots", [])
        if not turn_snapshots:
            raise HTTPException(status_code=400, detail="No turn snapshots found in session.")

        turn_df = pd.DataFrame(turn_snapshots)
        if turn_df.empty:
            raise HTTPException(status_code=400, detail="Turn snapshots dataframe is empty.")
        
        turn_col = "turn" if "turn" in turn_df.columns else "Turn"
        turn_df = turn_df[turn_df[turn_col] <= 16].copy()

        cluster_features = preprocess_turn_snapshot_cluster(turn_df)
        regression_features = preprocess_turn_snapshot_regression(turn_df)

        if cluster_features is None:
            raise HTTPException(status_code=400, detail="Not enough data for cluster preprocessing.")
        if regression_features is None:
            raise HTTPException(status_code=400, detail="Not enough data for regression preprocessing.")

        try:
            X_cluster = pd.DataFrame([cluster_features])[self.kmeans_feature_cols]
            X_cluster_scaled = self.kmeans_scaler.transform(X_cluster)
            cluster_id = int(self.kmeans_model.predict(X_cluster_scaled)[0])
            cluster_name = CLUSTER_LABEL_MAP.get(cluster_id, "Unknown")
            

            X_reg = pd.DataFrame([regression_features])[self.lr_feature_cols]
            X_reg_scaled = self.lr_scaler.transform(X_reg)
            predicted_population = float(self.lr_model.predict(X_reg_scaled)[0])
        except Exception as e:
            raise HTTPException(status_code=500, detail=f"Prediction failed: {str(e)}")

        return {
            "session_id": session_id,
            "cluster": cluster_name,
            "predicted_population": predicted_population,
            "avg_final_population": avg_final_population
        }

        