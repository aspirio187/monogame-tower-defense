# {Project Name} — Implementation Plan

## Context

{Why this plan exists, what we're building, current state}

---

## Design

{All specs, data tables, algorithms, constants. This is the reference material agents read.}

---

## Assets

{Status of all required assets. Mark DONE or TODO.}

---

## Agent Sections

Each section is owned by one agent. File ownership is explicit — only the owning agent may create/edit those files.

Shared files (e.g. a main orchestrator class) require lock coordination via `.locks/`.

---

### @{agent-name}

**Owns:** {files this agent creates or exclusively edits}
**Modifies:** {files owned by another agent that this agent also needs to edit}
**Touches (shared):** {shared files requiring lock}

- {What this agent does — requirements, not implementation steps}
- **Verify:** {how to confirm this section is done}

---

{Repeat for each agent}

---

## Execution Order

{Which agents can run in parallel, which must be sequential, and why.}

Group format:
1. **Parallel:** @agent-a + @agent-b (no file overlap)
2. **Sequential after group 1:** @agent-c (depends on files from group 1)

---

## Verification

{How to verify the full build after all agents complete.}
