from fastapi import APIRouter, HTTPException
from app.dependencies import machine_learning_service as ml_service

router = APIRouter()

@router.post("/{session_id}/predict")
def predict_all(session_id: str):
    return ml_service.predict_all(session_id)