# Converty Repository Operating Contract

## Main-first authority

`main` is the repository authority.

Durable product, source, test, documentation, build, packaging, or security work must not exist only in a local workspace, chat handover, or unmerged development branch. Push durable commits immediately to GitHub.

A side branch is a temporary surface for RED reproduction, diagnostics, or explicitly isolated experiments. A side-branch-only CI run is never final repository authority.

## Required working behavior

1. Before any write, fetch the live repository state and resolve the current `main` HEAD.
2. Put durable work on GitHub as soon as it becomes a durable commit. Do not accumulate completed work only in a local workspace.
3. Use side branches only when temporary isolation is technically necessary. Keep their purpose explicit.
4. Do not call a tranche complete because a side branch is green.
5. Do not auto-merge or auto-promote arbitrary experimental branches just to satisfy this rule.
6. Preserve failed/RED evidence; do not rewrite history to hide failed qualification attempts.
7. After generated authority changes, synchronize only the deterministic runner-generated authority bytes. Do not hand-edit generated SBOM/package/hash authority.

## Final qualification gate

Before any completion, freeze, promotion, or handover claim:

- fetch the live default branch again;
- prove the qualified SHA is the current `main` HEAD;
- prove ordinary CI ran on that exact SHA and completed successfully;
- prove tracked generated authority is current/zero-diff when that gate applies;
- record the exact commit SHA, tree, workflow run/job IDs, artifact IDs/digests, and workspace evidence available from that run.

If the qualified SHA is not the current `main` HEAD, the work is development-only and must not be represented as completed repository authority.

## Product invariants

This repository workflow rule does not change Converty runtime architecture or shipping policy. Preserve the existing Explorer → Bridge → strict disposable worker/provider → FFmpeg → transactional publication path, security boundaries, no-shell/no-PATH converter rules, source/destination preservation rules, and production-signing/FFmpeg-redistribution limitations.
