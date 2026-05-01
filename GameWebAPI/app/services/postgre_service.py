import psycopg2
from psycopg2.extras import Json
from app.models.dto import TurnSnapshotDto
from app.models.TurnActionSummaryDto import TurnActionSummaryDto


class PostgresService:
    def __init__(self):
        self.conn = psycopg2.connect(
            host="localhost",
            database="postgres",
            user="postgres",
            password="teoh0628",
            port=5432
        )

    def save_session(self, session: dict):
        with self.conn.cursor() as cur:
            cur.execute("""
                INSERT INTO gameplay_sessions (
                    session_id,
                    player_id,
                    started_at_utc
                )
                VALUES (%s, %s, %s)
                ON CONFLICT (session_id) DO NOTHING;
            """, (
                session["session_id"],
                session["player_id"],
                session["started_at_utc"]
            ))

        self.conn.commit()

    def save_turn_snapshot(self, session_id: str, snapshot: TurnSnapshotDto):
        if isinstance(snapshot, dict):
            snapshot = TurnSnapshotDto(**self.unity_snapshot_to_snake(snapshot))

        with self.conn.cursor() as cur:
            cur.execute("""
                INSERT INTO turn_snapshots (
                    session_id,
                    turn,
                    gold,
                    population,
                    total_supply_provided,
                    ap,
                    ap_used,
                    upgrade_count,
                    demolish_count,
                    small_house_count,
                    big_house_count,
                    supply_count,
                    service_count,
                    factory_count,
                    road_count,
                    average_satisfaction_index,
                    average_pollution_index,
                    average_service_index,
                    houses_near_factory_count,
                    houses_without_service_count,
                    houses_low_satisfaction_count,
                    total_tax_income
                )
                VALUES (
                    %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s,
                    %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s
                );
            """, (
                session_id,
                snapshot.turn,
                snapshot.gold,
                snapshot.population,
                snapshot.total_supply_provided,
                snapshot.ap,
                snapshot.ap_used,
                snapshot.upgrade_count,
                snapshot.demolish_count,
                snapshot.small_house_count,
                snapshot.big_house_count,
                snapshot.supply_count,
                snapshot.service_count,
                snapshot.factory_count,
                snapshot.road_count,
                snapshot.average_satisfaction_index,
                snapshot.average_pollution_index,
                snapshot.average_service_index,
                snapshot.houses_near_factory_count,
                snapshot.houses_without_service_count,
                snapshot.houses_low_satisfaction_count,
                snapshot.total_tax_income
            ))

        self.conn.commit()

    # def save_turn_action_summary(
    #     self,
    #     session_id: str,
    #     summary: TurnActionSummaryDto
    # ):
    #     if isinstance(summary, dict):
    #         summary = TurnActionSummaryDto(**self.unity_summary_to_snake(summary))

    #     with self.conn.cursor() as cur:
    #         cur.execute("""
    #             INSERT INTO turn_action_summaries (
    #                 session_id,
    #                 turn,
    #                 actions_taken,
    #                 buildings_placed,
    #                 upgrades,
    #                 demolitions
    #             )
    #             VALUES (%s, %s, %s, %s, %s, %s);
    #         """, (
    #             session_id,
    #             summary.turn,
    #             summary.actions_taken,
    #             Json([item.model_dump() for item in summary.buildings_placed]),
    #             Json([item.model_dump() for item in summary.upgrades]),
    #             Json([item.model_dump() for item in summary.demolitions])
    #         ))

    #     self.conn.commit()

    def save_turn_data(
        self,
        session_id: str,
        snapshot: TurnSnapshotDto,
        summary: TurnActionSummaryDto
    ):
        self.save_turn_snapshot(session_id, snapshot)
        # self.save_turn_action_summary(session_id, summary)

    def unity_snapshot_to_snake(self, data: dict) -> dict:
        return {
            "turn": data["Turn"],
            "gold": data["Gold"],
            "population": data["Population"],
            "total_supply_provided": data["TotalSupplyProvided"],
            "ap": data["AP"],
            "ap_used": data["APUsed"],
            "upgrade_count": data["UpgradeCount"],
            "demolish_count": data["DemolishCount"],

            "small_house_count": data["SmallHouseCount"],
            "big_house_count": data["BigHouseCount"],
            "supply_count": data["SupplyCount"],
            "service_count": data["ServiceCount"],
            "factory_count": data["FactoryCount"],
            "road_count": data["RoadCount"],

            "average_satisfaction_index": data["AverageSatisfactionIndex"],
            "average_pollution_index": data["AveragePollutionIndex"],
            "average_service_index": data["AverageServiceIndex"],

            "houses_near_factory_count": data["HousesNearFactoryCount"],
            "houses_without_service_count": data["HousesWithoutServiceCount"],
            "houses_low_satisfaction_count": data["HousesLowSatisfactionCount"],

            "total_tax_income": data["TotalTaxIncome"],
        }

    def unity_summary_to_snake(self, data: dict) -> dict:
        return {
            "turn": data["Turn"],
            "actions_taken": data["ActionsTaken"],
            "buildings_placed": [
                {
                    "building_type": x["building_type"] if "building_type" in x else x["BuildingType"],
                    "position": {
                        "x": x["position"]["x"] if "position" in x else x["Position"]["x"],
                        "y": x["position"]["y"] if "position" in x else x["Position"]["y"]
                    }
                }
                for x in data.get("BuildingsPlaced", [])
            ],
            "upgrades": [
                {
                    "building_type": x.get("building_type", x["BuildingType"]),
                    "position": {
                        "x": x.get("position", x["Position"])["x"],
                        "y": x.get("position", x["Position"])["y"]
                    },
                    "from_level": x.get("from_level", x["FromLevel"]),
                    "to_level": x.get("to_level", x["ToLevel"])
                }
                for x in data.get("Upgrades", [])
            ],
            "demolitions": [
                {
                    "building_type": x.get("building_type", x["BuildingType"]),
                    "position": {
                        "x": x.get("position", x["Position"])["x"],
                        "y": x.get("position", x["Position"])["y"]
                    }
                }
                for x in data.get("Demolitions", [])
            ]
        }