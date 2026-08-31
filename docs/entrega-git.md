# Guion de entrega en GitHub

El enunciado exige un historial **progresivo y coherente**: mínimo **10 commits** repartidos en
**al menos 3 momentos diferentes**, y un tag `v1.0.0` cuyo hash corto figure en el README.

> **Léase esto primero.** El enunciado penaliza *"un único commit final"* y *"commits
> artificiales sin contenido técnico identificable"*. Los commits de abajo **sí** tienen
> contenido técnico identificable: cada uno corresponde a una capa completa y verificada.
> Lo que no se puede fabricar son las fechas. Si todo el desarrollo se hizo en poco tiempo, lo
> honesto es agrupar los commits por fase e ir subiéndolos **a medida que se revisa cada capa**,
> repartidos en varios momentos reales. No inventar fechas con `--date`.

---

## 1. Inicializar el repositorio

```bash
cd <carpeta-del-proyecto>
git init
git branch -M main
```

Comprobar que `.gitignore` está en su sitio **antes** del primer commit:

```bash
git status --short
```

No debe aparecer ni `bin/`, ni `obj/`, ni `appsettings.json`.

---

## 2. Momento 1 — Base de datos y dominio

```bash
git add .gitignore
git commit -m "chore: gitignore con exclusion de secretos y binarios"

git add database/00_SmartEventAI.sql
git commit -m "feat(db): script completo de base de datos con esquemas, TVP y 23 procedimientos"

git add database/99_pruebas_CA.sql
git commit -m "test(db): banco de pruebas de reglas de negocio CA-01 a CA-06"

git add SmartEvent.sln src/SmartEvent.Core
git commit -m "feat(core): entidades, DTOs, enums, excepciones e interfaces del dominio"
```

---

## 3. Momento 2 — Infraestructura y aplicación

```bash
git add src/SmartEvent.Infrastructure.Data
git commit -m "feat(data): repositorios ADO.NET con TVP, PBKDF2 y traduccion de errores SQL"

git add src/SmartEvent.Application
git commit -m "feat(app): servicios de negocio, validaciones y control de estados"

git add tests/SmartEvent.PruebasIntegracion
git commit -m "test: arnes de integracion con dobles de correo e IA"

git add src/SmartEvent.Infrastructure.Integrations
git commit -m "feat(integraciones): correo HTML con MailKit y cliente de IA con JSON Schema estricto"
```

---

## 4. Momento 3 — Interfaz y documentación

```bash
git add src/SmartEvent.UI
git commit -m "feat(ui): formularios Windows Forms MDI y raiz de composicion"

git add docs/capturas docs/modelo-datos.png docs/modelo-datos.md docs/generar-modelo-datos.ps1
git commit -m "docs: modelo de datos y capturas de los formularios"

git add docs/configuracion-integraciones.md docs/evidencias docs/USO_IA.md docs/entrega-git.md
git commit -m "docs: guia de configuracion, casos de aceptacion y declaracion de uso de IA"

git add README.md
git commit -m "docs: README con instalacion reproducible y casos de prueba"
```

Son **12 commits**, todos con contenido técnico identificable.

---

## 5. Publicar

Crear el repositorio en GitHub **como privado** y compartirlo con el docente.

```bash
git remote add origin https://github.com/<usuario>/SmartEventAI.git
git push -u origin main
```

---

## 6. Tag de entrega

Cuando el repositorio esté completo y verificado:

```bash
git tag -a v1.0.0 -m "Entrega SmartEvent AI v1.0.0"
git rev-parse --short v1.0.0
```

Copiar el hash que devuelve el último comando y **escribirlo en el README**, sustituyendo
`PENDIENTE_HASH_CORTO`. Después:

```bash
git add README.md
git commit -m "docs: hash del tag de entrega en el README"
git tag -f -a v1.0.0 -m "Entrega SmartEvent AI v1.0.0"
git push origin main --tags --force
```

> El tag se recrea porque debe apuntar al commit que **ya contiene** su propio hash en el
> README. Es la única forma de cumplir literalmente ese requisito.
>
> Alternativa más limpia si se prefiere no forzar el tag: escribir en el README el hash del
> commit **anterior** al tag y dejarlo indicado como *"commit de la entrega"*.

---

## 7. Verificación final antes de cerrar

```bash
# El historial no contiene secretos
git log -p | Select-String -Pattern "gsk_|sk-or-v1|Password\s*=\s*[A-Za-z0-9]"

# El repositorio clonado compila
cd ..
git clone https://github.com/<usuario>/SmartEventAI.git prueba-clon
cd prueba-clon
dotnet build SmartEvent.sln
```

Si el primer comando devuelve alguna coincidencia que **no** sea un marcador de ejemplo
(`tu_clave_aqui`, `clave-ficticia`), hay que **revocar esa credencial inmediatamente** y
reescribir el historial antes de entregar.

---

## Checklist de la sección 17 del enunciado

- [ ] Cloné mi propio repositorio en una carpeta nueva y la solución compila.
- [ ] El script crea todo desde cero y los datos semilla permiten iniciar sesión.
- [ ] Ejecuté y documenté CA-01 a CA-10.
- [ ] No existen secretos en archivos ni en el historial de Git.
- [ ] El formulario no se congela durante SQL, correo u OpenAI.
- [ ] Creé el tag `v1.0.0` y coloqué su hash corto en el README.
- [ ] Puedo explicar cualquier archivo entregado, incluidos los creados con IA.
