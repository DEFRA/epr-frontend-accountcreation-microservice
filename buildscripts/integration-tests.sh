#!/usr/bin/env bash

dotnet test src/FrontendAccountCreation.IntegrationTests/FrontendAccountCreation.IntegrationTests.csproj --logger "trx;logfilename=testResults.trx"