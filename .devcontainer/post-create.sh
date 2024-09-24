#!/bin/sh

# Restore .NET tools
dotnet tool restore
dotnet dev-certs https --trust
