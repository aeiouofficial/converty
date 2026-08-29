# Main Authority Continuity Guard Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Prevent Converty work from being treated as complete while the default `main` branch is stale or the qualified commit exists only on a side branch.

**Architecture:** Add a small repository-continuity verifier with focused static tests, wire it into GitHub Actions as a push-only non-main authority gate, and codify the same main-first rule in `AGENTS.md`. The gate detects ancestry only; it never auto-merges or changes product runtime behavior.

**Tech Stack:** Python 3.13, pytest 9.0.2, Git, GitHub Actions YAML.

**Spec:** `docs/superpowers/specs/2026-08-29-main-authority-continuity-design.md`

## Global Constraints

- `main` is the repository authority.
- Durable work must be committed and pushed to GitHub immediately.
- Side branches are temporary RED/diagnostic/explicit-isolation surfaces, not final authority.
- A non-main push ahead of `main` must fail `main-authority-continuity`.
- Pull-request events remain usable.
- No auto-merge or auto-promotion.
- No product runtime, Explorer, Bridge/Host/Worker, FFmpeg, signing, or release-semantics changes.

---

### Task 1: RED continuity contract tests

**Files:**
- Create: `tests/static/test_main_authority_continuity.py`

**Interfaces:**
- Consumes: repository files `AGENTS.md`, `.github/workflows/ci.yml`, and module `scripts.verify_main_continuity`.
- Produces: executable acceptance criteria for the verifier and workflow contract.

- [ ] **Step 1: Write failing tests**

Create tests that import `scripts.verify_main_continuity.verify_main_continuity` and assert:

```python
assert verify_main_continuity(event_name="push", ref_name="main", head_sha="abc", is_ancestor=lambda *_: False).ok
assert verify_main_continuity(event_name="pull_request", ref_name="feature/x", head_sha="abc", is_ancestor=lambda *_: False).ok
assert verify_main_continuity(event_name="push", ref_name="dev/x", head_sha="abc", is_ancestor=lambda *_: True).ok
result = verify_main_continuity(event_name="push", ref_name="dev/x", head_sha="abc", is_ancestor=lambda *_: False)
assert not result.ok
assert "development-only" in result.message.lower()
```

Also assert `AGENTS.md` contains `main` authority language and final live verification requirements, and `.github/workflows/ci.yml` contains `main-authority-continuity`, `fetch-depth: 0`, and `python scripts/verify_main_continuity.py`.

- [ ] **Step 2: Run focused test and prove RED**

Run:

```bash
python -m pytest -q tests/static/test_main_authority_continuity.py
```

Expected: FAIL because the verifier/contract/job do not exist.

- [ ] **Step 3: Commit RED evidence**

```bash
git add tests/static/test_main_authority_continuity.py
git commit -m "test: define main authority continuity gate"
git push origin main
```

### Task 2: Implement verifier and agent contract

**Files:**
- Create: `scripts/verify_main_continuity.py`
- Create: `AGENTS.md`

**Interfaces:**
- Produces: `ContinuityResult(ok: bool, message: str)` and `verify_main_continuity(event_name, ref_name, head_sha, is_ancestor)`.
- CLI consumes GitHub environment variables and uses `git merge-base --is-ancestor HEAD origin/main` through `subprocess.run`.

- [ ] **Step 1: Implement minimal verifier**

Behavior:

```python
if event_name != "push":
    return ContinuityResult(True, "Pull-request/review event; push authority gate not applicable.")
if ref_name == "main":
    return ContinuityResult(True, "Current push is on main repository authority.")
if is_ancestor(head_sha, "origin/main"):
    return ContinuityResult(True, "Branch HEAD is already contained in main.")
return ContinuityResult(False, "Development-only branch is ahead of main; promote to main and qualify that exact SHA before completion.")
```

CLI exits `0` for PASS and `1` for FAIL.

- [ ] **Step 2: Add `AGENTS.md`**

Require future agents to:

- use `main` as durable authority;
- push durable commits immediately;
- use side branches only for temporary RED/diagnostic/isolation;
- never report completion from a side-branch-only CI run;
- finish by fetching live `main`, proving the qualified SHA is `main` HEAD, and citing exact CI evidence.

- [ ] **Step 3: Run focused tests**

```bash
python -m pytest -q tests/static/test_main_authority_continuity.py
```

Expected: workflow assertion still FAIL because CI is not yet wired; verifier/AGENTS assertions PASS.

- [ ] **Step 4: Commit implementation**

```bash
git add scripts/verify_main_continuity.py AGENTS.md
git commit -m "feat: enforce main-first repository authority contract"
git push origin main
```

### Task 3: Wire GitHub Actions continuity gate

**Files:**
- Modify: `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: `scripts/verify_main_continuity.py`.
- Produces: `main-authority-continuity` workflow job.

- [ ] **Step 1: Add continuity job**

Add an Ubuntu job with full history:

```yaml
  main-authority-continuity:
    runs-on: ubuntu-24.04
    timeout-minutes: 5
    steps:
      - uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1
        with:
          persist-credentials: false
          fetch-depth: 0
      - name: Verify default-branch authority continuity
        env:
          CONTINUITY_EVENT_NAME: ${{ github.event_name }}
          CONTINUITY_REF_NAME: ${{ github.ref_name }}
          CONTINUITY_HEAD_SHA: ${{ github.sha }}
        run: |
          git fetch --no-tags origin +refs/heads/main:refs/remotes/origin/main
          python scripts/verify_main_continuity.py
```

- [ ] **Step 2: Run focused tests**

```bash
python -m pytest -q tests/static/test_main_authority_continuity.py
```

Expected: PASS.

- [ ] **Step 3: Run all static tests**

```bash
python -m pytest -q tests/static
```

Expected: all static tests PASS.

- [ ] **Step 4: Commit CI gate**

```bash
git add .github/workflows/ci.yml
git commit -m "ci: reject side-branch-only final authority"
git push origin main
```

### Task 4: Final generated-authority and exact-main qualification

**Files:**
- Regenerated by existing CI authority workflow: `machine-readable/source_sbom.spdx.json`, `machine-readable/release_sbom.spdx.json`, `machine-readable/package_manifest.json`, `SHA256SUMS.txt` as applicable.

**Interfaces:**
- Consumes: existing deterministic generators and final source tree.
- Produces: current tracked generated authority and exact-main qualification evidence.

- [ ] **Step 1: Inspect CI on exact `main` SHA**

Require the new continuity job to PASS on `main`, focused/static gates to PASS, and record any expected generated-authority freshness RED caused by source changes.

- [ ] **Step 2: Sync exact generated authority using existing deterministic authority procedure if stale**

Do not hand-edit SBOM/package/hash files. Use the generated artifact bytes and preserve atomicity.

- [ ] **Step 3: Trigger no-tree-change final qualifier if bot-generated authority commit does not trigger CI**

The qualifier must preserve the generated-authority tree exactly.

- [ ] **Step 4: Verify final live repository state**

Fetch `main` directly and require:

- live `main` HEAD equals the qualified SHA;
- `main-authority-continuity` PASS;
- ordinary static and managed CI PASS;
- tracked generated authority zero diff;
- final workspace/delivery artifact PASS.

- [ ] **Step 5: Record handover**

Report exact SHA/tree/run/job/artifact/workspace evidence. Never call a side-branch-only run final authority.
