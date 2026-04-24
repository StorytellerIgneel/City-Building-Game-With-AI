from dataclasses import dataclass
from typing import Optional


@dataclass
class ReactionResponseDto:
    status: str
    reaction: Optional[str]


@dataclass
class AdviceResponseDto:
    status: str
    advice: Optional[str]


@dataclass
class ObjectiveResponseDto:
    status: str
    objective_type: Optional[str]
    difficulty: Optional[str]
    reason: Optional[str]