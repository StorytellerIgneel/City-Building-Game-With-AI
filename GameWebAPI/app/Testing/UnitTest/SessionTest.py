# tests/test_session_service.py

import pytest
from app.services.session_service import SessionService


class FakeDto:
    def __init__(self, value):
        self.value = value

    def model_dump(self):
        return {"value": self.value}


def test_create_session():
    service = SessionService()

    session = service.create_session("player_001")

    assert session["session_id"] is not None
    assert session["player_id"] == "player_001"
    assert session["started_at_utc"] is not None
    assert session["action_logs"] == []
    assert session["turn_snapshots"] == []
    assert session["turn_action_summaries"] == []

    assert session["session_id"] in service.sessions


def test_session_exists_returns_true_for_existing_session():
    service = SessionService()
    session = service.create_session("player_001")

    result = service.session_exists(session["session_id"])

    assert result is True


def test_session_exists_returns_false_for_invalid_session():
    service = SessionService()

    result = service.session_exists("invalid_session_id")

    assert result is False


def test_get_session_returns_existing_session():
    service = SessionService()
    session = service.create_session("player_001")

    result = service.get_session(session["session_id"])

    assert result == session


def test_get_session_returns_none_for_invalid_session():
    service = SessionService()

    result = service.get_session("invalid_session_id")

    assert result is None


def test_add_action_log():
    service = SessionService()
    session = service.create_session("player_001")

    action_log = FakeDto("build_house")

    service.add_action_log(session["session_id"], action_log)

    assert len(session["action_logs"]) == 1
    assert session["action_logs"][0] == {"value": "build_house"}


def test_add_turn_snapshot():
    service = SessionService()
    session = service.create_session("player_001")

    snapshot = FakeDto("turn_1_snapshot")

    service.add_turn_snapshot(session["session_id"], snapshot)

    assert len(session["turn_snapshots"]) == 1
    assert session["turn_snapshots"][0] == snapshot


def test_add_turn_snapshot_invalid_session_raises_error():
    service = SessionService()
    snapshot = FakeDto("turn_1_snapshot")

    with pytest.raises(ValueError, match="Session not found."):
        service.add_turn_snapshot("invalid_session_id", snapshot)


def test_add_turn_action_summary():
    service = SessionService()
    session = service.create_session("player_001")

    summary = FakeDto("turn_1_summary")

    service.add_turn_action_summary(session["session_id"], summary)

    assert len(session["turn_action_summaries"]) == 1
    assert session["turn_action_summaries"][0] == summary


def test_add_turn_data():
    service = SessionService()
    session = service.create_session("player_001")

    snapshot = FakeDto("turn_1_snapshot")
    summary = FakeDto("turn_1_summary")

    service.add_turn_data(session["session_id"], snapshot, summary)

    assert len(session["turn_snapshots"]) == 1
    assert len(session["turn_action_summaries"]) == 1
    assert session["turn_snapshots"][0] == snapshot
    assert session["turn_action_summaries"][0] == summary


def test_add_turn_data_invalid_session_raises_error():
    service = SessionService()

    snapshot = FakeDto("turn_1_snapshot")
    summary = FakeDto("turn_1_summary")

    with pytest.raises(ValueError, match="Session not found."):
        service.add_turn_data("invalid_session_id", snapshot, summary)