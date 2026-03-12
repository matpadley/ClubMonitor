# Codecov guidance removed

Per repository owner request, Codecov integration instructions were intentionally removed. The CI workflow no longer calls the Codecov action. Coverage artifacts (Cobertura XML and the generated HTML report) are still produced and uploaded as workflow artifacts for manual inspection.

If you change your mind later and want Codecov re-enabled, follow these high-level steps:

- Add the repository to Codecov and obtain an upload token (if required).
- Store the token in the GitHub Actions secrets as `CODECOV_TOKEN`.
- Add or re-enable the Codecov action step in `.github/workflows/ci.yml` to upload coverage files.



