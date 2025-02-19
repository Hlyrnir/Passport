#### Description

Passport can be used for authorization in a Mediator project. For each message, a required visa can be defined, and only if the holder is authorized, the message will be processed.

- PassportVisa: For every endpoint you can specify a mandatory visa. This visa has to be issued to a passport in order to authorize any request.
- Passport: A passport has to be enabled and valid to generate a authentication token.
- PassportHolder: Every passport is issued to a specific holder.
- PassportToken: Credentials at a specific provider will generate a token. This token is needed for generating a authentication token.

For realisation following concepts has been used.

- Clean architecture
- Domain driven design
- Message pipeline (Request -> Authorization -> Validation -> Handler -> Result -> Response) using [Mediator](https://github.com/martinothamar/Mediator)
- ORM using [Dapper](https://github.com/DapperLib/Dapper)