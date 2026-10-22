# Sujex ERP + CRM — Complete Project Context

## 1. Product identity

Sujex is a multi-tenant ERP and CRM workspace for small and medium businesses. It brings customer relationships, sales, purchasing, inventory, finance, HR and project delivery into one system.

The product is designed to be used by business owners, operations teams, sales teams, finance teams, warehouse staff, HR managers and project managers. A tenant represents one customer organisation. A tenant can contain one or more companies and branches, users, roles and business records.

The application is currently prepared as a client-demo-ready product with a seeded demonstration tenant, realistic sample records, guided page explanations and editable detail views.

## 2. Why we built it

Businesses often manage customers in one tool, inventory in another, invoices in spreadsheets, employee information in email and project work in a separate task tool. That creates duplicate data, unclear ownership, manual reconciliation and slow decisions.

Sujex is built to provide:

- One reliable source of operational data.
- A connected flow from lead to opportunity, order, invoice and payment.
- Stock visibility across products and warehouses.
- Consistent employee, leave and department records.
- Project budgets, tasks, priorities and delivery status.
- Role-based access and tenant isolation.
- A simple daily workspace that a new user can understand without training sessions.

## 3. Main benefits

### Business benefits

- Faster daily decisions from the dashboard.
- Less duplicate entry between CRM, sales, inventory and finance.
- Better follow-up discipline for leads and customer activities.
- Clear ownership of records and tasks.
- Traceable purchasing, stock movements and payments.
- Consistent reporting across departments.
- A foundation that can support multiple customer organisations.

### User benefits

- Searchable lists instead of spreadsheets.
- Detail pages that show the complete record.
- Inline guidance explaining what each page means.
- Clear important-field guidance.
- Edit and save actions for core records.
- Statuses that show where work is in the process.
- Demo data that makes the workflow easy to understand.

## 4. How the system works

The user opens the Next.js frontend. The frontend calls the ASP.NET Core API using Axios. The API authenticates the user with ASP.NET Core Identity and JWT tokens, resolves the tenant from the token, applies tenant-safe data access and reads/writes PostgreSQL through Entity Framework Core.

High-level flow:

1. User opens the frontend and signs in.
2. The API validates the credentials.
3. The API returns an access token and refresh token.
4. The frontend stores the access token for the active session.
5. Requests include the bearer token.
6. Tenant middleware resolves the tenant and company context.
7. Controllers validate the request and call the data layer.
8. Entity Framework Core reads or saves PostgreSQL data.
9. TanStack Query refreshes affected lists and detail views after mutations.
10. The UI shows success, loading and error states.

Important local demo URLs:

- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

The API must be started from `backend/src/ErpCrm.API` so ASP.NET Core loads the correct appsettings files and connection configuration.

## 5. Application areas

### Dashboard

Purpose: the daily control centre.

It shows summary KPIs for contacts, open leads, opportunities, sales orders, products, employees, projects and pending leave requests.

Daily use:

1. Start here.
2. Check unusual or urgent numbers.
3. Open the related module for action.
4. Refresh after important changes.

### CRM

#### Contacts

A contact is a person at a customer, prospect or partner.

Important fields:

- First name and last name.
- Email or phone.
- Company/account.
- Job title and department.
- Active status.
- Notes and tags.

Typical flow:

1. Search for an existing contact.
2. Open the contact name to see details.
3. Edit information when it changes.
4. Use the contact in lead, opportunity and sales activity workflows.

#### Leads

A lead is unqualified business interest.

Important fields:

- Lead title.
- Person and company details.
- Status.
- Source.
- Estimated value.
- Follow-up date.
- Notes and tags.

Typical flow:

1. Capture the lead.
2. Set its source and follow-up date.
3. Update status as sales qualifies it.
4. Convert a qualified lead into an opportunity.

#### Opportunities

An opportunity is a qualified deal being worked by sales.

Important fields:

- Title and stage.
- Expected value.
- Probability.
- Expected close date.
- Owner.
- Notes.
- Lost reason when applicable.

Typical flow:

1. Create after lead qualification.
2. Keep stage, probability and value current.
3. Review weighted value for forecasting.
4. Close as won or lost with a clear reason.

#### Activities

Activities are calls, meetings, tasks and follow-ups.

Important fields:

- Activity type.
- Subject.
- Related contact, lead or opportunity.
- Due date.
- Owner.
- Completion status.

Typical flow:

1. Create the activity against the relevant record.
2. Set the owner and due date.
3. Complete it immediately after the work is done.

### Sales

#### Sales orders

A sales order represents confirmed customer demand.

Important fields:

- Customer/contact.
- Order date.
- Status.
- Product lines.
- Quantity and price.
- Discount and tax.
- Expected delivery date.
- Total and notes.

Typical flow:

1. Select the customer.
2. Add products and quantities.
3. Check totals and delivery date.
4. Progress the status through fulfilment and invoicing.

#### Sales invoices

An invoice requests payment from the customer.

Important fields:

- Customer.
- Invoice date.
- Due date.
- Line items.
- Tax and totals.
- Paid amount.
- Balance due.
- Status.

Typical flow:

1. Create or review the invoice from the sale.
2. Verify tax and totals.
3. Record payments.
4. Keep the balance and status accurate.

### Inventory

#### Products

Products are the goods or services sold or purchased.

Important fields:

- Name.
- Code and SKU.
- Category and unit.
- Cost price.
- Sale price.
- Minimum, maximum and reorder levels.
- Active/service flags.
- Description and tags.

Typical flow:

1. Create the product.
2. Assign category and unit.
3. Maintain prices and stock thresholds.
4. Use stock movements for physical stock changes.

#### Stock movements

Stock movements are the auditable quantity changes behind inventory balances.

Important fields:

- Product.
- Warehouse.
- Movement type.
- Quantity.
- Unit cost.
- Reference number/type.
- Notes.

Typical flow:

1. Select the product and warehouse.
2. Choose receipt, issue, adjustment or transfer.
3. Enter quantity and cost.
4. Add a traceable reference.

#### Warehouses

Warehouses identify physical or logical storage locations.

Important fields:

- Name.
- Code.
- Company or branch.
- Address.
- Active status.

#### Categories

Categories group products for filtering, navigation and reporting.

Important fields:

- Name.
- Code.
- Parent category.
- Description.

### Purchase

#### Vendors

Vendors are suppliers of goods or services.

Important fields:

- Name and code.
- Email and phone.
- Tax number.
- Currency.
- Payment terms.
- Active status.
- Notes.

#### Purchase orders

Purchase orders authorise and track buying from vendors.

Important fields:

- Vendor.
- Warehouse.
- Order date.
- Expected delivery date.
- Product lines.
- Quantities and costs.
- Approval and order status.
- Notes.

Typical flow:

1. Select the vendor.
2. Add products and quantities.
3. Check expected delivery.
4. Approve, receive and close the order.

### HR

#### Employees

Employee records are the source for people, leave and accountability information.

Important fields:

- Name.
- Employee code.
- Email and phone.
- Join date.
- Department and designation.
- Manager.
- Employment status.
- Basic salary.
- National ID and bank account where permitted.

#### Departments

Departments provide organisational structure.

Important fields:

- Name and code.
- Company.
- Manager.
- Active status.

#### Leave requests

Leave requests are the approval queue for employee absence.

Important fields:

- Employee.
- Leave type.
- Start and end dates.
- Total days.
- Reason.
- Status.

Typical flow:

1. Review dates and reason.
2. Check the employee context.
3. Approve or reject.
4. Keep the decision for audit.

### Finance

#### Accounts

Accounts form the chart of accounts.

Important fields:

- Account code.
- Account name.
- Account type.
- Parent account.
- Balance.
- Active status.
- System-account flag.

#### Journal entries

Journal entries record balanced accounting transactions.

Important fields:

- Entry date.
- Description.
- Reference.
- Currency.
- Debit lines.
- Credit lines.
- Status.

Typical flow:

1. Create the header.
2. Add debit and credit lines.
3. Confirm the entry balances.
4. Post only after review.

### Projects

Projects group delivery work, budgets and tasks.

Important fields:

- Name and code.
- Company.
- Description.
- Start and end dates.
- Status.
- Budget.
- Manager.
- Active status.

Tasks contain:

- Title.
- Description.
- Status.
- Priority.
- Due date.
- Owner.
- Estimated and actual hours.

Typical flow:

1. Create the project.
2. Set dates, budget and manager.
3. Add tasks.
4. Keep task status, priority and hours current.

### Settings

Settings controls the company profile, users and roles.

Use settings to:

- Maintain company name, tax details, address, currency and timezone.
- Review users and active status.
- Assign roles.
- Review permissions.
- Configure the operating context before a client demo.

## 6. Detail and edit behaviour

Core records can be opened from linked names in list tables. Detail views show the important information in one place.

Editable detail views are available for:

- Contacts.
- Leads.
- Opportunities.
- Products.
- Employees.
- Projects and project tasks.

The edit panel:

1. Opens with the existing values.
2. Marks required fields.
3. Supports text, email, number, date, select and notes fields.
4. Validates required values before saving.
5. Sends the update to the API.
6. Refreshes the detail and list data after a successful save.
7. Shows an error if the API rejects the update.

For records without a dedicated detail editor, use the page’s create, status or settings actions. The page guide explains the intended workflow and important fields.

## 7. Client demonstration story

A simple client demo should follow one connected story:

1. Sign in with the demo account.
2. Show the dashboard and explain the KPIs.
3. Open Contacts and show a realistic contact.
4. Open Leads, update a lead and convert it to an opportunity.
5. Open the opportunity and update value, probability or stage.
6. Open Products and show price, stock and reorder details.
7. Record or review a stock movement.
8. Create or review a sales order and invoice.
9. Show a vendor and purchase order.
10. Show an employee and leave request.
11. Show a project, add a task and update its priority.
12. Finish in Settings to explain users, roles and company configuration.

Demo credentials:

- Email: `demo@demo-technology.test`
- Password: `Demo@12345`

These credentials are for local demonstration only and must be changed or removed before production use.

## 8. Technical architecture

### Frontend

- Next.js 15 App Router.
- React and TypeScript.
- Tailwind CSS.
- TanStack Query for server state and cache invalidation.
- Zustand for authentication/session state.
- Axios API client.
- Shared UI components for buttons, badges, page guides and record editors.

Important frontend folders:

- `src/app`: routes and pages.
- `src/components/layout`: application shell and sidebar.
- `src/components/ui`: reusable visual components.
- `src/hooks`: API query and mutation hooks.
- `src/lib`: API client and utilities.
- `src/types`: API-facing TypeScript models.

### Backend

- ASP.NET Core 8 Web API.
- Clean architecture.
- Entity Framework Core.
- PostgreSQL through Npgsql and Supabase.
- ASP.NET Core Identity.
- JWT access tokens and refresh-token rotation.
- Serilog logging.
- Tenant-resolution middleware.
- Swagger/OpenAPI.

Important backend projects:

- `ErpCrm.API`: controllers, middleware, startup and configuration.
- `ErpCrm.Application`: DTOs, interfaces and application contracts.
- `ErpCrm.Domain`: entities, enums and domain rules.
- `ErpCrm.Infrastructure`: EF Core, persistence, authentication, email and seeding.

## 9. Security and tenancy

Tenant isolation is a core product requirement.

- Every tenant-owned record has a tenant identifier.
- Tenant middleware resolves the current tenant from the authenticated request.
- Global query filters prevent cross-tenant reads.
- JWT claims carry identity and tenant context.
- Roles and policies control administrative actions.
- Secrets and connection strings must remain in environment/configuration files and must never be committed.

Production requirements:

- Replace demo credentials.
- Use a production JWT secret.
- Store data-protection keys in durable protected storage.
- Configure secure CORS origins.
- Use HTTPS.
- Protect database and Supabase service credentials.
- Review role permissions and audit logging.

## 10. Local development

### Frontend

```powershell
cd frontend
npm install
npm run dev -- -H 0.0.0.0 -p 3000
```

### Backend

```powershell
cd backend/src/ErpCrm.API
dotnet run --urls http://0.0.0.0:5000
```

The backend must run from the API project folder so the correct appsettings file is loaded.

### Validation

```powershell
cd frontend
npm run type-check
npm run lint
npm run build

cd ../backend
dotnet build ErpCrm.sln
dotnet test
```

## 11. Troubleshooting

### Browser cannot reach localhost

- Confirm the frontend process is still running.
- Use port 3000.
- Start Next.js with `-H 0.0.0.0`.
- Check the frontend output log.
- Reload the browser after the server reports Ready.

### API starts but database is unavailable

- Run from `backend/src/ErpCrm.API`.
- Confirm the connection string exists in the environment or appsettings configuration.
- Check `api-server-host.out.log`.
- Confirm Supabase is reachable.
- Do not replace production credentials with demo values.

### Login works but lists are empty

- Confirm the API seeded the demo tenant.
- Sign out and sign in again to refresh tenant claims.
- Check browser network errors and API logs.
- Confirm the current user has access to the relevant module.

### A save does not appear immediately

- Wait for the success response.
- Refresh the page.
- Check the browser and API logs.
- Confirm the record is not deleted or inactive.

## 12. Current delivery status

Completed for the client demo:

- Main ERP and CRM navigation.
- Dashboard KPI loading.
- API and frontend integration.
- Database migrations and demo seed data.
- Dummy records across the core modules.
- Page-level explanations and usage guidance.
- Core detail views and editing.
- API error banner and loading/error states.
- Frontend type-check and lint validation.
- Backend build and seeded-login verification.

Areas to expand next:

- Dedicated detail/edit screens for every transaction and finance record.
- More granular permissions and role-specific navigation.
- Automated end-to-end browser tests.
- Production deployment and monitoring.
- Exporting reports and documents.
- More complete audit-history views.

