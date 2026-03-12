# SmartInventory | Documentação

**Versão:** 1.0

**Objetivo:** descrever o sistema Smart Inventory do ponto de vista funcional e técnico (backend e frontend).

---

## Sumário

1. Parte funcional
2. Backend
3. Frontend

---

# 1. Parte funcional

## 1.1 Visão geral

O Smart Inventory é um sistema corporativo de controle de estoque. Ele registra produtos, movimentações (entradas/saídas), controla níveis mínimos e exige aprovação administrativa quando uma movimentação deixaria o estoque abaixo do mínimo.

## 1.2 Perfis de acesso

- **Administrador**
  - Pode cadastrar/editar/inativar produtos.
  - Pode cadastrar/editar/inativar tipos de movimentação.
  - Pode cadastrar/editar/inativar usuários.
  - Pode aprovar/rejeitar solicitações pendentes.
  - Pode registrar movimentações.

- **Operador**
  - Pode visualizar produtos e histórico de movimentações.
  - Pode registrar movimentações.
  - Não acessa aprovações nem usuários.
  - Não altera tipos de movimentação.

## 1.3 Funcionalidades principais

### Produtos

- Listar produtos ativos (com opção **Exibir inativos**).
- Visualizar status de estoque (OK / Abaixo do mínimo).
- Cadastrar e editar produtos.
- Inativar produtos (soft delete).
- Filtrar por **baixo estoque**.

### Movimentações

- Registrar movimentações com tipo, quantidade e motivo (quando manual).
- Consultar histórico com filtros por produto, usuário, tipo e período.
- Exibir o estoque antes/depois da movimentação.
- Regras de aprovação quando o estoque final ficaria abaixo do mínimo.

### Aprovações

- Listar solicitações pendentes.
- Aprovar ou rejeitar solicitações.
- Aprovação exige confirmação explícita do administrador.

### Tipos de movimentação

- Cadastrar tipos com código e descrição.
- Exemplo: Entrada, Saída, Venda, Ajuste.
- Editar e inativar tipos.

### Usuários

- Cadastrar novos usuários.
- Alterar perfil (Administrador/Operador).
- Ativar/Inativar.
- Exibir inativos quando solicitado.

## 1.4 Regras de negócio

- **Quantidade movimentada** deve ser maior que zero.
- **Movimentação manual** exige motivo obrigatório.
- **Estoque não pode ficar negativo.**
- **Se o estoque final ficar abaixo do mínimo:**
  - Administrador pode aplicar a movimentação com confirmação.
  - Operador gera solicitação de aprovação.
- Solicitações expiram em **7 dias**.

## 1.5 Inativação

- Produtos e usuários são **inativados**, não excluídos.
- Itens inativos são ocultos por padrão no frontend.
- Usuário pode optar por **Exibir inativos**.

---

# 2. Backend

## 2.1 Stack e arquitetura

- **.NET** (Minimal API)
- **Entity Framework Core**
- **PostgreSQL**
- Autenticação por **Cookie**

Ponto de entrada: `Program.cs`

## 2.2 Autenticação e autorização

- Cookie: `smartinventory.auth`
- Expiração: 8h (sliding)
- Login: `/auth/login`
- Política de autorização:
  - `AdminOnly` (role `Admin`)

## 2.3 Estrutura de domínio (Entidades)

- **Product**
  - `Name`, `Sku`, `MinimumQuantity`, `CurrentQuantity`, `IsActive`
- **User**
  - `Name`, `Email`, `PasswordHash`, `Role`, `IsActive`
- **MovementType**
  - `Code`, `Description`, `StockEffect`
- **InventoryMovement**
  - `QuantityMoved`, `LastInventoryQuantity`, `CurrentInventoryQuantity`, `IsManual`, `Reason`, `MovementDate`
- **MovementApprovalRequest**
  - `QuantityMoved`, `Reason`, `Status`, `ExpiresAt`, `ProposedInventoryQuantity`

## 2.4 Endpoints

### Auth

- `POST /auth/login` (público)
- `POST /auth/logout` (autenticado)
- `GET /auth/me` (autenticado)

### Produtos

- `GET /products` (autenticado)
- `GET /products/low-stock` (autenticado)
- `POST /products` (Admin)
- `PATCH /products/{id}` (Admin)

### Movimentações

- `GET /movements` (autenticado)
  - filtros: `productId`, `userId`, `movementTypeId`, `startDate`, `endDate`
- `POST /movements` (autenticado)

### Aprovações

- `GET /approvals/pending` (Admin)
- `POST /approvals/{id}/approve` (Admin)
- `POST /approvals/{id}/reject` (Admin)

### Tipos de movimentação

- `GET /movement-types` (autenticado)
- `POST /movement-types` (Admin)
- `PATCH /movement-types/{id}` (Admin)
- `DELETE /movement-types/{id}` (Admin)

### Usuários

- `GET /users` (Admin)
- `POST /users` (Admin)
- `PATCH /users/{id}/role` (Admin)
- `PATCH /users/{id}/active` (Admin)
- `DELETE /users/{id}/disable` (Admin)

## 2.5 Fluxo de movimentação

1. Recebe solicitação de movimentação.
2. Valida quantidade, motivo (se manual) e tipo.
3. Calcula impacto no estoque (`StockEffect`).
4. Se o estoque final ficar abaixo do mínimo:
   - Admin aplica imediatamente com confirmação.
   - Operador gera solicitação de aprovação.
5. Se estoque final >= mínimo: aplica normalmente.

## 2.6 Erros e respostas

O backend usa `ServiceResult<T>`:

- `Success: true/false`
- `StatusCode`
- `Error` (mensagem)
- `Data`

Erros inesperados são tratados pelo `GlobalExceptionMiddleware` retornando:

```json
{ "message": "Erro interno do servidor." }
```

## 2.7 Configuração

`appsettings.json`:

- `ConnectionStrings:DefaultConnection`
- `SeedAdmin`: usuário administrador inicial
