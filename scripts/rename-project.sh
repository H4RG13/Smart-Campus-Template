#!/bin/bash
# Renames the SmartCampus.* solution/projects/namespaces to a new school-specific
# name — the "Rename Project" step in the clone-and-deploy workflow (README.md,
# docs/ARCHITECTURE.md §10). Intended to be a ~10-minute mechanical task per
# RULES.md, not a design decision — this script is what makes it actually take
# 10 minutes instead of an afternoon of manual find-and-replace.
#
# Usage: scripts/rename-project.sh <NewName>
#   e.g. scripts/rename-project.sh Riverside
#        renames SmartCampus.Api -> Riverside.Api, SmartCampus.Domain -> Riverside.Domain, etc.
#
# Scope: solution/project files, C# namespaces, docker-compose service naming
# references (Jwt Issuer/Audience), firmware project references, client/package.json.
# Deliberately does NOT touch docs/*.md, README.md, PLAN.md, RULES.md — those are
# template documentation about the architecture, not identifiers the code depends
# on; update their prose manually afterward if you want full white-labeling.
set -euo pipefail

OLD_NAME="SmartCampus"
NEW_NAME="${1:-}"

if [ -z "$NEW_NAME" ]; then
  echo "Usage: $0 <NewName>" >&2
  echo "  <NewName> must be a valid C# identifier (PascalCase, no spaces), e.g. Riverside" >&2
  exit 1
fi

if ! [[ "$NEW_NAME" =~ ^[A-Za-z][A-Za-z0-9]*$ ]]; then
  echo "Error: '$NEW_NAME' is not a valid identifier (letters/digits only, starting with a letter)." >&2
  exit 1
fi

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$REPO_ROOT"

echo "==> Renaming '$OLD_NAME' to '$NEW_NAME' in $REPO_ROOT"
echo "    (solution/src/tests/firmware/deploy/client — not docs/*.md, README.md, PLAN.md, RULES.md)"
echo

# --- Step 1: text replacement inside files (before renaming paths, so grep/sed
# targets are still found at their original locations) ---
TARGET_DIRS=(src tests firmware deploy)
TARGET_FILES=("SmartCampus.Template.sln" "client/package.json" "client/package-lock.json")

echo "==> Replacing '$OLD_NAME' -> '$NEW_NAME' inside file contents"
for dir in "${TARGET_DIRS[@]}"; do
  if [ -d "$dir" ]; then
    grep -rlZ "$OLD_NAME" "$dir" --include="*.cs" --include="*.csproj" --include="*.sln" \
      --include="*.json" --include="*.yml" --include="*.yaml" --include="*.cpp" --include="*.h" \
      --include="Dockerfile*" 2>/dev/null \
      | xargs -0 -r sed -i "s/$OLD_NAME/$NEW_NAME/g" || true
  fi
done
for file in "${TARGET_FILES[@]}"; do
  if [ -f "$file" ] && grep -q "$OLD_NAME" "$file" 2>/dev/null; then
    sed -i "s/$OLD_NAME/$NEW_NAME/g" "$file"
  fi
done

# --- Step 2: rename files and directories containing the old name, deepest first
# so renaming a parent directory doesn't invalidate already-computed child paths ---
echo "==> Renaming files and directories"
find . -depth \( -path "./.git" -o -path "*/node_modules" -o -path "*/node_modules/*" \) -prune \
  -o -name "*$OLD_NAME*" -print 2>/dev/null | while IFS= read -r path; do
  new_path="$(dirname "$path")/$(basename "$path" | sed "s/$OLD_NAME/$NEW_NAME/g")"
  if [ "$path" != "$new_path" ]; then
    mkdir -p "$(dirname "$new_path")"
    git mv "$path" "$new_path" 2>/dev/null || mv "$path" "$new_path"
    echo "    $path -> $new_path"
  fi
done

echo
echo "==> Done. Next steps:"
echo "    1. dotnet build   (verify the renamed solution compiles)"
echo "    2. cd client && npm run build   (verify the frontend still builds)"
echo "    3. Review docs/*.md, README.md manually if you want full white-labeling"
echo "    4. Configure branding + environment variables per README.md's deploy workflow"
