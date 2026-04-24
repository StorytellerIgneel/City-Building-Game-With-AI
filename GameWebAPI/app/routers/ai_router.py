from fastapi import APIRouter, WebSocket, WebSocketDisconnect, HTTPException
from app.dependencies import session_service, session_websocket_service
import structlog

router = APIRouter()
logger = structlog.get_logger()


@router.get("/{session_id}")
def get_session(session_id: str):
    session = session_service.get_session(session_id)
    if not session:
        raise HTTPException(status_code=404, detail="Session not found.")
    return session

@router.websocket("/ws/{session_id}")
async def session_websocket(websocket: WebSocket, session_id: str):
    if not session_service.session_exists(session_id):
        await websocket.close(code=1008)
        return

    await websocket.accept()
    logger.info("WebSocket connected", session_id=session_id)

    try:
        while True:
            message = await websocket.receive_json()
            await session_websocket_service.handle_message(
                websocket=websocket,
                session_id=session_id,
                message=message
            )

    except WebSocketDisconnect:
        logger.info("WebSocket disconnected", session_id=session_id)

    except Exception as e:
        logger.exception("WebSocket error", session_id=session_id, error=str(e))
        try:
            await websocket.send_json({
                "type": "error",
                "message": "Internal server error."
            })
        except Exception:
            pass