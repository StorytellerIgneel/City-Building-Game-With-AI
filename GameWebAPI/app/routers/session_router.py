from fastapi import APIRouter, HTTPException

from app.models.TurnActionSummaryDto import TurnActionSummaryDto
from app.models.TurnDataRequest import TurnDataRequest
from app.models.dto import (
    StartSessionRequest,
    StartSessionResponse,
    ActionLogDto,
    TurnSnapshotDto,
)
from fastapi import APIRouter, HTTPException
from typing import Dict, Any
from app.dependencies import session_service

router = APIRouter()
@router.post("/start", response_model=StartSessionResponse)
def start_session(request: StartSessionRequest):
    session = session_service.create_session(request.player_id)

    return StartSessionResponse(
        session_id=session["session_id"],
        started_at_utc=session["started_at_utc"],
        message="Session started successfully."
    )

@router.get("/{session_id}")
def get_session(session_id: str):
    session = session_service.get_session(session_id)
    if not session:
        raise HTTPException(status_code=404, detail="Session not found.")

    return session

@router.post("/{session_id}/turn-data")
def receive_turn_data(session_id: str, request_body: Dict[str, Any]):
    if not session_service.session_exists(session_id):
        raise HTTPException(status_code=404, detail="Session not found.")

    turn_snapshot = request_body.get("turn_snapshot")
    turn_action_summary = request_body.get("turn_action_summary")

    if turn_snapshot is None:
        raise HTTPException(status_code=400, detail="Missing turn_snapshot data.")

    if turn_action_summary is None:
        raise HTTPException(status_code=400, detail="Missing turn_action_summary data.")

    session_service.add_turn_data(session_id, turn_snapshot, turn_action_summary)

    return {"message": "Turn data received."}