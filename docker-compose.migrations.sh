docker compose -f docker-compose.migrations.yml down
docker compose -f docker-compose.migrations.yml up --abort-on-container-exit --exit-code-from migrations migrations