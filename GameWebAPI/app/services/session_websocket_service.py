import json

from fastapi import WebSocket
import structlog


class SessionWebSocketService:
    def __init__(self, session_service, ai_service):
        self.session_service = session_service
        self.ai_service = ai_service
        self.logger = structlog.get_logger()

        self.handlers = {
            "ping": self._handle_ping,
            "GenerateReaction": self._handle_generate_reaction,
            "GenerateAdvice": self._handle_generate_advice,
            "InitializeAI": self._handle_initialize_ai,
            "GenerateObjective": self._handle_generate_objective,
            "custom_gemini_message": self._handle_custom_gemini_message,
        }

    async def handle_message(self, websocket: WebSocket,session_id: str,message: dict) -> None:
        message_type = message.get("type")
        handler = self.handlers.get(message_type)

        if handler is None:
            await self._send_error(
                websocket,
                f"Unknown message type: {message_type}"
            )
            return

        await handler(websocket, session_id, message)

    async def _handle_ping(self, websocket: WebSocket,session_id: str,message: dict) -> None:
        await websocket.send_json({
            "type": "pong"
        })

    async def _handle_action_log(self, websocket: WebSocket, session_id: str, message: dict ) -> None:
        payload = message.get("data")
        if payload is None:
            await self._send_error(websocket, "Missing action_log data.")
            return

        self.session_service.add_action_log_dict(session_id, payload)

        await websocket.send_json({
            "type": "ack",
            "message": "Action log received."
        })

    async def _handle_generate_reaction(self, websocket: WebSocket, session_id: str, message: dict) -> None:
        turn_snapshot = message.get("turnSnapshot")
        turn_action_summary = message.get("turnActionSummary")

        if turn_snapshot is None:
            await self._send_error(websocket, "Missing turnSnapshot data.")
            return

        if turn_action_summary is None:
            await self._send_error(websocket, "Missing turnActionSummary data.")
            return

        self.session_service.add_turn_data(session_id, turn_snapshot, turn_action_summary)

        ai_reaction_input = {
            "turn_snapshot": {
                "turn": turn_snapshot.get("Turn"),
                "gold": turn_snapshot.get("Gold"),
                "population": turn_snapshot.get("Population"),
                "total_supply_provided": turn_snapshot.get("TotalSupplyProvided"),
                "ap": turn_snapshot.get("AP"),
                "ap_used": turn_snapshot.get("APUsed"),
                "upgrade_count": turn_snapshot.get("UpgradeCount"),
                "demolish_count": turn_snapshot.get("DemolishCount"),
                "small_house_count": turn_snapshot.get("SmallHouseCount"),
                "big_house_count": turn_snapshot.get("BigHouseCount"),
                "supply_count": turn_snapshot.get("SupplyCount"),
                "service_count": turn_snapshot.get("ServiceCount"),
                "factory_count": turn_snapshot.get("FactoryCount"),
                "road_count": turn_snapshot.get("RoadCount"),
                "average_satisfaction_index": turn_snapshot.get("AverageSatisfactionIndex"),
                "average_pollution_index": turn_snapshot.get("AveragePollutionIndex"),
                "average_service_index": turn_snapshot.get("AverageServiceIndex"),
                "houses_near_factory_count": turn_snapshot.get("HousesNearFactoryCount"),
                "houses_without_service_count": turn_snapshot.get("HousesWithoutServiceCount"),
                "houses_low_satisfaction_count": turn_snapshot.get("HousesLowSatisfactionCount"),
                "total_tax_income": turn_snapshot.get("TotalTaxIncome")
            },
            "turn_action_summary": {
                "turn": turn_action_summary.get("Turn"),
                "actions_taken": turn_action_summary.get("ActionsTaken"),
                "buildings_placed": turn_action_summary.get("BuildingsPlaced", []),
                "upgrades": turn_action_summary.get("Upgrades", []),
                "demolitions": turn_action_summary.get("Demolitions", [])
            }
        }

        reaction_prompt = f"""
            Use case:
            Generate_reaction

            Turn snapshot:
            {json.dumps(ai_reaction_input["turn_snapshot"], indent=2)}

            Turn action summary:
            {json.dumps(ai_reaction_input["turn_action_summary"], indent=2)}
            """

        session = self.session_service.get_session(session_id)
        if session is None:
            await self._send_error(websocket, "Session not found.")
            return

        session.setdefault("ai_reaction_inputs", []).append(ai_reaction_input)

        print("AI Reaction Prompt:", reaction_prompt)

        result = self.ai_service.generate_turn_reaction(session, reaction_prompt)

        session.setdefault("ai_reaction_outputs", []).append({
            "status": result.status,
            "reaction": result.reaction
        })

        print("AI Reaction Output:", result)

        await websocket.send_json({
            "status": result.status,
            "reaction": result.reaction
        })

    async def _handle_initialize_ai(self, websocket: WebSocket, session_id: str, message: dict ) -> None:
        session = self._get_session_or_raise(session_id)
        self.ai_service.initialize_gemini_session(session)

        await websocket.send_json({
            "type": "ai_initialized",
            "message": "Gemini session initialized."
        })
    
    async def _handle_generate_advice(self, websocket: WebSocket, session_id: str, message: dict) -> None:
        advice_summary = message.get("adviceSummary")

        if advice_summary is None:
            await self._send_error(websocket, "Missing adviceSummary data.")
            return

        session = self.session_service.get_session(session_id)
        if session is None:
            await self._send_error(websocket, "Session not found.")
            return

        ai_advice_input = {
            "advice_summary": {
                "current_turn": advice_summary.get("currentTurn"),
                "avg_population_growth_last_3_turns": advice_summary.get("avgPopulationGrowthLast3Turns"),
                "avg_satisfaction_last_3_turns": advice_summary.get("avgSatisfactionLast3Turns"),
                "avg_pollution_last_3_turns": advice_summary.get("avgPollutionLast3Turns"),
                "avg_service_last_3_turns": advice_summary.get("avgServiceLast3Turns"),
                "avg_build_count_last_3_turns": advice_summary.get("avgBuildCountLast3Turns"),
            }
        }

        advice_prompt = f"""
    Use case:
    Generate_advice

    Advice summary:
    {json.dumps(ai_advice_input["advice_summary"], indent=2)}
    """

        session.setdefault("ai_advice_inputs", []).append(ai_advice_input)

        print("AI Advice Prompt:", advice_prompt)

        result = self.ai_service.generate_turn_advice(session, advice_prompt)

        session.setdefault("ai_advice_outputs", []).append({
            "status": result.status,
            "advice": result.advice
        })

        print("AI Advice Output:", result)

        await websocket.send_json({
            "status": result.status,
            "advice": result.advice
        })


    async def _handle_generate_objective(self, websocket: WebSocket, session_id: str, message: dict) -> None:
        player_cluster = message.get("playerCluster")
        estimated_population = message.get("estimatedPopulation")
        average_final_population = message.get("averageFinalPopulation")
        difficulty = message.get("difficulty", ["easy", "medium", "hard"])
        objective_types = message.get(
            "objectiveTypes",
            ["ReachPopulation", "BuildCount", "KeepPollutionBelow", "MaintainSatisfactionAbove"]
        )

        if player_cluster is None or player_cluster == "":
            await self._send_error(websocket, "Missing playerCluster data.")
            return

        if estimated_population is None:
            await self._send_error(websocket, "Missing estimatedPopulation data.")
            return

        session = self.session_service.get_session(session_id)
        if session is None:
            await self._send_error(websocket, "Session not found.")
            return

        ai_objective_input = {
            "objective_request": {
                "player_cluster": player_cluster,
                "estimated_population": estimated_population,
                "average_final_population": average_final_population,
                "difficulty_options": difficulty,
                "objective_type_options": objective_types
            }
        }

        objective_prompt = f"""
            Use case:
            Generate_objective

            Player cluster:
            {player_cluster}

            Estimated population:
            {estimated_population}

            Allowed objective types:
            {json.dumps(objective_types, indent=2)}

            Allowed difficulty options:
            {json.dumps(difficulty, indent=2)}

            Average final population:
            {average_final_population}

            Choose:
            - ONE objective_type from the allowed list
            - ONE difficulty from the allowed list
            - ONE short reason explaining why this objective fits the player's playstyle and estimated performance

            Return raw JSON only.
            Do not use markdown.
            Do not wrap the response in ```json.
            Do not include extra text.

            Required JSON format:
            {{
                "objective_type": "one allowed objective type",
                "difficulty": "one allowed difficulty",
                "reason": "short explanation"
            }}
            """

        session.setdefault("ai_objective_inputs", []).append(ai_objective_input)

        print("AI Objective Prompt:", objective_prompt)

        result = self.ai_service.generate_turn_objective(session, objective_prompt)

        # 🔒 Optional safety validation (VERY recommended)
        if result.objective_type not in objective_types:
            print("Invalid objective_type from AI:", result.objective_type)
            result.objective_type = objective_types[0]  # fallback

        if result.difficulty not in difficulty:
            print("Invalid difficulty from AI:", result.difficulty)
            result.difficulty = difficulty[0]  # fallback

        session.setdefault("ai_objective_outputs", []).append({
            "status": result.status,
            "objective_type": result.objective_type,
            "difficulty": result.difficulty,
            "reason": result.reason
        })

        print("AI Objective Output:", result)

        await websocket.send_json({
            "status": result.status,
            "objective_type": result.objective_type,
            "difficulty": result.difficulty,
            "reason": result.reason
        })

    async def _handle_custom_gemini_message(self, websocket: WebSocket, session_id: str, message: dict ) -> None:
        payload = message.get("message")
        if payload is None:
            await self._send_error(websocket, "Missing custom_gemini_message data.")
            return

        session = self._get_session_or_raise(session_id)
        result_text = self.ai_service.custom_gemini_message(
            session=session,
            custom_prompt=payload
        )

        await websocket.send_json({
            "type": "custom_gemini_response",
            "data": result_text
        })

    def _get_session_or_raise(self, session_id: str) -> dict:
        session = self.session_service.get_session(session_id)
        if not session:
            raise ValueError(f"Session not found: {session_id}")
        return session

    async def _send_error(self, websocket: WebSocket, message: str) -> None:
        await websocket.send_json({
            "type": "error",
            "message": message
        })