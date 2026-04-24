from pydantic import BaseModel
from typing import Optional
from datetime import datetime


class StartSessionRequest(BaseModel):
    player_id: str


class StartSessionResponse(BaseModel):
    session_id: str
    started_at_utc: datetime
    message: str


class ActionLogDto(BaseModel):
    turn: int
    time_since_session_start: float
    action_type: str
    building_type: Optional[str] = None
    position_x: int
    position_y: int
    gold_before: int
    gold_after: int
    ap_before: int
    ap_after: int
    was_valid: bool
    notes: Optional[str] = None


class TurnSnapshotDto(BaseModel):
    turn: int
    gold: int
    population: int
    total_supply_provided: int
    ap: int
    ap_used: int
    upgrade_count: int
    demolish_count: int

    small_house_count: int
    big_house_count: int
    supply_count: int
    service_count: int
    factory_count: int
    road_count: int

    average_satisfaction_index: float
    average_pollution_index: float
    average_service_index: float

    houses_near_factory_count: int
    houses_without_service_count: int
    houses_low_satisfaction_count: int

    total_tax_income: int


class RecommendObjectiveRequest(BaseModel):
    session_id: str