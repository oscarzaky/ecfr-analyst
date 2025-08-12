#!/bin/bash
# Wait for SQL Server to start
sleep 30s

# Run the initialization script
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P dogeECRd3m@ -i /docker-entrypoint-initdb.d/init-db.sql