#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 || -z "${1}" ]]; then
  echo "Uso: $0 <tag>"
  echo "Exemplo: $0 1.4.2"
  exit 1
fi

TAG="$1"
IMAGE="wallacevff/projeto-final:${TAG}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "${SCRIPT_DIR}"

echo ">> Build da imagem ${IMAGE}"
docker buildx build --no-cache -f Dockerfile -t "${IMAGE}" .

echo ">> Push da imagem ${IMAGE}"
docker push "${IMAGE}"

echo ">> Concluido: ${IMAGE}"
