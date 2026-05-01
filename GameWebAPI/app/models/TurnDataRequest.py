from pydantic import BaseModel

from app.models.TurnActionSummaryDto import TurnActionSummaryDto
from app.models.dto import TurnSnapshotDto


class TurnDataRequest(BaseModel):
    turn_snapshot: TurnSnapshotDto
    turn_action_summary: TurnActionSummaryDto