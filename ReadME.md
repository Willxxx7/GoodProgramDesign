# Maths Quiz — A Lesson in Good Program Design

A simple maths quiz for 6–9 year olds, written to demonstrate **good program
design principles**. This is not just a working program — it's a *teaching
artifact*.

Every design decision in the code is deliberate. This README explains what
those decisions are, why they matter, and how to use the code to teach.

---

## What the Program Does

A console-based multiplication quiz:

1. Asks for the player's name
2. Asks 10 multiplication questions (numbers 1–10)
3. Tracks the score
4. Gives encouraging feedback
5. Handles invalid input gracefully

Simple on the surface. Deliberate design underneath.

---

## How to Build and Run

### Requirements

- **.NET 10** (or later) — check with `dotnet --version`
- Any C# editor: Visual Studio, VS Code with the C# extension, or Rider

### Build

From the project folder:

```bash
dotnet build
```
