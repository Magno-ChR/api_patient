## Diagrama de clases

![Diagrama de clases](Docs/diagrama-clases.png)

## Linting y pre-commit

- **.editorconfig**: Configuración de estilo (tabs, no espacios; reglas C#). Aplicada por el editor y por `dotnet format`.
- **SonarLint**: Se recomienda instalar la extensión [SonarLint](https://marketplace.visualstudio.com/items?itemName=SonarSource.sonarlint-vscode) en VS Code (o SonarLint para Visual Studio). El proyecto sugiere la extensión en `.vscode/extensions.json`.
- **Husky + pre-commit**: Antes de cada commit se ejecuta:
  - **lint-staged**: aplica `dotnet format` a los `.cs` en stage según `.editorconfig` (incluye la regla de tabs).
  - **commit-msg** (commitlint): valida que el mensaje siga Conventional Commits (`feat:`, `fix:`, `chore:`, etc.).

### Configuración inicial

```bash
npm install
```

Con `npm install` se instalan Husky, lint-staged y commitlint. El script `prepare` configura los hooks de Husky automáticamente.