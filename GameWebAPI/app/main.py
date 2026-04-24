# entry point for backend server

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.routers.session_router import router as session_router
from app.routers.ai_router import router as ai_router
from app.routers.ml_router import router as ml_router


import structlog
# from app.routers.websocket_router import router as websocket_router

logger = structlog.get_logger(__name__)

app = FastAPI(title="Game AI Backend")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # tighten this later if needed
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(session_router, prefix="/session", tags=["Session"])
app.include_router(ai_router, prefix="/ai", tags=["AI"])
app.include_router(ml_router, prefix="/ml", tags=["ML"])

for route in app.routes:
    print(route.path)

logger.info("Game AI Backend server initialized.")