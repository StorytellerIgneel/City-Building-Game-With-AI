import json
import re
from requests import session
import structlog
from app.models.ResponseDTO import (
    ReactionResponseDto,
    AdviceResponseDto,
    ObjectiveResponseDto,
)

class AiService:
    logger = structlog.get_logger()

    def __init__(self, gemini_service):
        self.gemini = gemini_service

    def initialize_gemini_session(self, session: dict):
        system_prompt = """
            You are an AI director for a city-building game.

            Core Rules:
            - Supply should support population.
            - Service improves satisfaction.
            - Pollution reduces efficiency and satisfaction.
            - High service/satisfaction = good, high pollution = bad.
            - Houses near factories risk pollution.
            - Houses without service risk low satisfaction.

            Playstyles:
            - Expander: rapid housing growth.
            - Planner: balanced development.
            - Tempo: factory/AP focused.

            Input may include:
            - turn_snapshot
            - turn_action_summary
            - recent_turns
            - playstyle_label
            - estimated_performance

            Each request includes a "use_case":
            - Generate_reaction
            - Generate_advice
            - Generate_objective (only expected at turn 15)

            ---
            Generate_reaction:
            - Return only the reaction sentence as plain text.
            - Exactly 1 short sentence.
            - In-world tone: news, citizen, or bulletin.
            - No JSON.
            - No markdown.
            - No labels like "reaction:".
            - No numbers, no analysis.

            Generate_advice:
            - Return only the advice as plain text.
            - 1-2 sentences.
            - Practical and actionable.
            - No JSON.
            - No markdown.
            - No labels like "advice:".

            """

        result = self.gemini.generate_with_memory(prompt=system_prompt)

        print("Gemini session initialized with interaction ID:", result["interaction_id"])

        session["gemini_interaction_id"] = result["interaction_id"]

        self.logger.info(
            "Initialized Gemini session",
            interaction_id=result["interaction_id"]
        )

    def generate_turn_reaction(self, session: dict, custom_prompt: str) -> ReactionResponseDto:
        try: 
            result = self.gemini.generate_with_memory(
                prompt=custom_prompt,
                previous_interaction_id=session.get("gemini_interaction_id")
            )
        except Exception as e:
            self.logger.error("Gemini generation failed, returning fallback reaction", error=str(e))
            session["gemini_interaction_id"] = None
            return ReactionResponseDto(
                status="error",
                reaction="The city is evolving. Keep building and adapting!"
            )

        self.update_interaction_id(session, result)

        raw_text = result["text"].strip()

        return ReactionResponseDto(
            status="success",
            reaction=raw_text
        )
        
    def generate_turn_advice(self, session: dict, custom_prompt: str) -> AdviceResponseDto:
        try:
            result = self.gemini.generate_with_memory(
                prompt=custom_prompt,
                previous_interaction_id=session.get("gemini_interaction_id")
            )
        except Exception as e:
            self.logger.error("Gemini generation failed, returning fallback advice", error=str(e))
            session["gemini_interaction_id"] = None
            return AdviceResponseDto(
                status="error",
                advice="Focus on balancing housing and factories, and consider adding more services to improve satisfaction."
            )

        self.update_interaction_id(session, result)

        raw_text = result["text"].strip()

        return AdviceResponseDto(
            status="success",
            advice=raw_text
        )

    def generate_turn_objective(self, session: dict, custom_prompt: str) -> ObjectiveResponseDto:
        try:
            result = self.gemini.generate_with_memory(
                    prompt=custom_prompt,
                    previous_interaction_id=session.get("gemini_interaction_id")
                )
        except Exception as e:
            self.logger.error("Gemini generation failed, returning fallback objective", error=str(e))
            session["gemini_interaction_id"] = None  # reset interaction ID to avoid continued failures in future interactions
            return ObjectiveResponseDto(
                status="error",
                objective_type="ReachPopulation",
                difficulty="Easy",
                reason="Failed to generate objective."
            )

        self.update_interaction_id(session, result)

        raw_text = result["text"]
        cleaned_text = self._clean_json_text(raw_text)

        try:
            parsed_output = json.loads(cleaned_text)

            return ObjectiveResponseDto(
                status="success",
                objective_type=parsed_output.get("objective_type"),
                difficulty=parsed_output.get("difficulty"),
                reason=parsed_output.get("reason")
            )
        except json.JSONDecodeError:
            self.logger.error("Failed to parse Gemini objective output as JSON", output=raw_text)
            return ObjectiveResponseDto(
                status="error",
                objective_type="ReachPopulation",
                difficulty="Easy",
                reason=raw_text
            )
    
    def update_interaction_id(self, session, result):
        session["gemini_interaction_id"] = result["interaction_id"]

    def _clean_json_text(self, text: str) -> str:
        if not text:
            return text

        text = text.strip()

        # Remove markdown code fences if model accidentally adds them
        if text.startswith("```"):
            text = re.sub(r"^```(?:json)?\s*", "", text)
            text = re.sub(r"\s*```$", "", text)

        return text.strip()