#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")"

input="deck.md"
output="deck.pdf"

if ! command -v pandoc >/dev/null 2>&1; then
  echo "pandoc is required to build ${output}." >&2
  exit 1
fi

latex_engine="${LATEX_ENGINE:-}"

if [[ -z "${latex_engine}" ]]; then
  for candidate in tectonic xelatex lualatex pdflatex; do
    if command -v "${candidate}" >/dev/null 2>&1; then
      latex_engine="${candidate}"
      break
    fi
  done
fi

if [[ -z "${latex_engine}" ]]; then
  echo "A LaTeX engine is required to build ${output}. Install tectonic, xelatex, lualatex, or pdflatex." >&2
  exit 1
fi

pandoc "${input}" \
  --standalone \
  --to beamer \
  --slide-level 1 \
  --pdf-engine "${latex_engine}" \
  --output "${output}"

echo "Wrote ${output}"
