from datetime import datetime, timezone
from uuid import uuid4
from typing import Dict, Any

from app.models.dto import ActionLogDto, TurnSnapshotDto
from app.models.TurnActionSummaryDto import TurnActionSummaryDto
from app.services.postgre_service import PostgresService

class SessionService:
    def __init__(self):
        self.sessions: Dict[str, Dict[str, Any]] = {}
        self.postgres_service = PostgresService()

    def create_session(self, player_id: str) -> dict:
        session_id = str(uuid4())

        session = {
            "session_id": session_id,
            "player_id": player_id,
            "started_at_utc": datetime.now(timezone.utc),
            "action_logs": [],
            "turn_snapshots": [],
            "turn_action_summaries": [],
        }

        self.sessions[session_id] = session
        self.postgres_service.save_session(session)
        
        return session

    def session_exists(self, session_id: str) -> bool:
        return session_id in self.sessions

    def add_action_log(self, session_id: str, action_log: ActionLogDto):
        self.sessions[session_id]["action_logs"].append(action_log.model_dump())

    def add_turn_snapshot(self, session_id: str, snapshot: TurnSnapshotDto):
        if session_id not in self.sessions:
            raise ValueError("Session not found.")

        self.sessions[session_id]["turn_snapshots"].append(snapshot)

        # debug print
        print("Saved turn snapshot:")
        print(self.sessions[session_id]["turn_snapshots"])

    def add_turn_action_summary(self, session_id: str, turn_action_summary: TurnActionSummaryDto):
        self.sessions[session_id]["turn_action_summaries"].append(turn_action_summary)

    def add_turn_data(self, session_id: str,snapshot: TurnSnapshotDto,turn_action_summary: TurnActionSummaryDto):
        if session_id not in self.sessions:
            raise ValueError("Session not found.")

        self.sessions[session_id]["turn_snapshots"].append(snapshot)
        self.sessions[session_id]["turn_action_summaries"].append(turn_action_summary)

        self.postgres_service.save_turn_data(
            session_id,
            snapshot,
            turn_action_summary
        )

        print("Saved turn snapshot:")
        print(self.sessions[session_id]["turn_snapshots"])

    def get_session(self, session_id: str):
        return self.sessions.get(session_id)