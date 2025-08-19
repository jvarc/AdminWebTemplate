### 📄 `frontend/README.md`

````md
# AdminWebTemplate – Frontend (Angular 20)

## Tech

- Angular 20 (standalone + OnPush + strict TS)
- PrimeNG + PrimeIcons
- Tailwind v4 con `tailwindcss-primeui` (sin `tailwind.config.js`)
- Token JWT en **sessionStorage**

## Instalar y ejecutar

```bash
npm install
npm start       # ng serve
# abre http://localhost:4200
Configuración de estilos
src/styles.css:

css
Copiar
Editar
@plugin 'tailwindcss-primeui';
@layer tailwind, primeng;
@layer tailwind { @import "tailwindcss"; }
En cada *.component.css donde uses @apply:

css
Copiar
Editar
@reference "../../../styles.css";
Environments
src/environments/development.ts:

ts
Copiar
Editar
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5186' // ajusta si tu backend usa otro puerto
};
Autenticación y permisos
Login (email+password) → guarda el JWT en sessionStorage.

Interceptor añade Authorization: Bearer <token>.

Guards:

authGuard: exige sesión válida.

permissionGuard: exige perm específico del JWT.

El header muestra el menú según permisos.

Módulos incluidos
auth/ – login y helpers

admin/ – usuarios (CRUD activar/inactivar) y roles (listado)

dashboard/

shared/header/

core/guards, core/interceptors, core/services

Problemas comunes
401/403: revisa apiUrl, CORS del backend y expiración del token.

Estilos: confirma styles.css como arriba y que no exista tailwind.config.js.
```
````
