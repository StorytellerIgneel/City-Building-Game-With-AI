from fastapi import APIRouter, HTTPException

from app.models.dto import (
    StartSessionRequest,
    StartSessionResponse,
    ActionLogDto,
    TurnSnapshotDto,
)
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

@router.post("/{session_id}/action-log")
def receive_action_log(session_id: str, action_log: ActionLogDto):
    if not session_service.session_exists(session_id):
        raise HTTPException(status_code=404, detail="Session not found.")

    session_service.add_action_log(session_id, action_log)
    return {"message": "Action log received."}

@router.post("/{session_id}/turn-snapshot")
def receive_turn_snapshot(session_id: str, snapshot: TurnSnapshotDto):
    if not session_service.session_exists(session_id):
        raise HTTPException(status_code=404, detail="Session not found.")

    session_service.add_turn_snapshot(session_id, snapshot)
    return {"message": "Turn snapshot received."}


@router.get("/{session_id}")
def get_session(session_id: str):
    session = session_service.get_session(session_id)
    if not session:
        raise HTTPException(status_code=404, detail="Session not found.")

    return session