#!/usr/bin/env bash
set -euo pipefail

PROJECT_NAME="wuiam"
SERVICE_NAME="wuiam"
IMAGE_NAMES=("wuiam-wuiam:latest" "wuiam_wuiam:latest")
CONTAINER_NAMES=("wuiam-app" "wuiam_wuiam" "wuiam_wuiam_1" "wuiam-wuiam-1")

if docker compose version >/dev/null 2>&1; then
  COMPOSE=(docker compose -p "$PROJECT_NAME")
else
  COMPOSE=(docker-compose -p "$PROJECT_NAME")
fi

remove_stale_containers() {
  for container_name in "${CONTAINER_NAMES[@]}"; do
    if docker container inspect "$container_name" >/dev/null 2>&1; then
      echo "🧹 Removing stale container: $container_name"
      docker rm -f "$container_name"
    fi
  done
}

echo "📥 Pulling latest code..."
git pull origin main

echo "🛠️ Rebuilding Docker image..."
remove_stale_containers
"${COMPOSE[@]}" down --remove-orphans
remove_stale_containers

for image_name in "${IMAGE_NAMES[@]}"; do
  if docker image inspect "$image_name" >/dev/null 2>&1; then
    echo "🧹 Removing old image: $image_name"
    docker image rm -f "$image_name"
  fi
done

"${COMPOSE[@]}" build --pull "$SERVICE_NAME"

echo "🚀 Starting containers..."
"${COMPOSE[@]}" up -d --force-recreate --remove-orphans "$SERVICE_NAME"

echo "✅ Deployment complete."
