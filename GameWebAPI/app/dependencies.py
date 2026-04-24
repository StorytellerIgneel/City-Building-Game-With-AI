from app.services.session_service import SessionService
from app.services.ai_service import AiService
from app.services.gemini_service import GeminiService
from app.services.session_websocket_service import SessionWebSocketService
from app.services.machine_learning_service import MachineLearningService

session_service = SessionService()
gemini_service = GeminiService()
ai_service = AiService(gemini_service)
session_websocket_service = SessionWebSocketService(
    session_service=session_service,
    ai_service=ai_service
)
machine_learning_service = MachineLearningService(
    session_service=session_service,
    gemini_service=gemini_service
)