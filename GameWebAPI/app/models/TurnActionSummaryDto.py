from pydantic import BaseModel
from typing import List


class PointDto(BaseModel):
    x: int
    y: int


class BuildActionDto(BaseModel):
    building_type: str
    position: PointDto


class UpgradeActionDto(BaseModel):
    building_type: str
    position: PointDto
    from_level: int
    to_level: int


class DemolishActionDto(BaseModel):
    building_type: str
    position: PointDto


class TurnActionSummaryDto(BaseModel):
    turn: int
    actions_taken: int

    buildings_placed: List[BuildActionDto] = []
    upgrades: List[UpgradeActionDto] = []
    demolitions: List[DemolishActionDto] = []