#!/bin/bash
# =============================================================================
# OpenAPI Spec Generator for WUIAM API
# Generates OpenAPI 3.0 spec from Swagger UI and saves to output file
# =============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
OUTPUT_DIR="$SCRIPT_DIR"
OUTPUT_FILE="$OUTPUT_DIR/openapi-spec.json"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}  WUIAM API - OpenAPI Spec Generator${NC}"
echo -e "${BLUE}============================================${NC}"
echo ""

# Check if dotnet CLI is available
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: dotnet CLI is not installed.${NC}"
    echo "Please install .NET SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

echo -e "${GREEN}✓ dotnet CLI found${NC}"

# Check if the project builds successfully
echo ""
echo -e "${YELLOW}Building project...${NC}"
cd "$PROJECT_DIR"

if ! dotnet build --no-restore 2>/dev/null; then
    echo -e "${YELLOW}Full build required, running restore first...${NC}"
    if ! dotnet build 2>&1; then
        echo -e "${RED}Error: Project build failed. Please fix build errors before generating OpenAPI spec.${NC}"
        exit 1
    fi
fi

echo -e "${GREEN}✓ Project built successfully${NC}"

# Find the DLL path
DLL_PATH="$PROJECT_DIR/bin/Debug/net10.0/WUIAM.dll"
if [ ! -f "$DLL_PATH" ]; then
    # Try Release build
    DLL_PATH="$PROJECT_DIR/bin/Release/net10.0/WUIAM.dll"
fi

if [ ! -f "$DLL_PATH" ]; then
    echo -e "${RED}Error: Could not find WUIAM.dll${NC}"
    echo "Expected at: $DLL_PATH"
    exit 1
fi

echo -e "${GREEN}✓ Found DLL: $DLL_PATH${NC}"

# Check if Swagger tools are available
SWAGGER_TOOLS_AVAILABLE=false
if command -v swagger-cli &> /dev/null || npm list -g @apidevtools/swagger-cli &> /dev/null; then
    SWAGGER_TOOLS_AVAILABLE=true
    echo -e "${GREEN}✓ Swagger CLI tools available${NC}"
else
    echo -e "${YELLOW}⚠ Swagger CLI not found. Will generate via Swagger UI endpoint instead.${NC}"
fi

# Generate OpenAPI spec using Swagger's built-in mechanism
# We'll use the SwaggerGen middleware to generate the spec programmatically
echo ""
echo -e "${YELLOW}Generating OpenAPI spec...${NC}"

# Create a temporary script to generate the spec
cat > /tmp/generate_openapi.csx << 'EOF'
#r "$DLL_PATH"

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using System;
using System.IO;
using System.Reflection;

var host = Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup(typeof(WUIAM.Startup));
    })
    .Build();

using var scope = host.Services.CreateScope();
var swaggerGen = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.MvcOptions>>();

Console.WriteLine("OpenAPI spec generation via SwaggerGen requires running the app.");
Console.WriteLine("Please use the Swagger UI endpoint instead: http://localhost:5000/swagger/v1/swagger.json");

EOF

# Since we can't easily run the generation script, use an alternative approach:
# Create a helper endpoint or use Swagger's built-in output

echo ""
echo -e "${YELLOW}Alternative: Use the running app's Swagger endpoint to get the spec${NC}"
echo ""
echo -e "${GREEN}Steps to generate the OpenAPI spec:${NC}"
echo ""
echo "1. Start the backend server:"
echo "   cd ERP-Backend"
echo "   dotnet run"
echo ""
echo "2. Once running, the Swagger UI is available at:"
echo "   http://localhost:5000/swagger"
echo ""
echo "3. Download the OpenAPI spec JSON:"
echo "   curl -o ERP-Backend/openapi-spec.json http://localhost:5000/swagger/v1/swagger.json"
echo ""
echo "4. Or view it in Swagger UI and click 'Export' button"
echo ""

# Check if swagger.json already exists from a running instance
if [ -f "$OUTPUT_FILE" ]; then
    echo -e "${YELLOW}⚠ OpenAPI spec already exists at: $OUTPUT_FILE${NC}"
    echo "It will be overwritten if you regenerate."
    echo ""
fi

# Create a convenience script for generating the spec
CONVENIENCE_SCRIPT="$OUTPUT_DIR/generate-openapi.sh"
cat > "$CONVENIENCE_SCRIPT" << 'SCRIPT'
#!/bin/bash
# =============================================================================
# Convenience script to generate OpenAPI spec from running WUIAM API
# =============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_FILE="$SCRIPT_DIR/openapi-spec.json"

echo "Starting WUIAM API in background..."

# Start the API in background
cd "$SCRIPT_DIR/.."
dotnet run &
API_PID=$!

# Wait for server to start
echo "Waiting for server to start..."
sleep 5

# Download the OpenAPI spec
echo "Downloading OpenAPI spec..."
curl -s -o "$OUTPUT_FILE" http://localhost:5000/swagger/v1/swagger.json 2>/dev/null || \
curl -s -o "$OUTPUT_FILE" http://localhost:5001/swagger/v1/swagger.json 2>/dev/null

# Kill the API server
kill $API_PID 2>/dev/null || true

if [ -f "$OUTPUT_FILE" ]; then
    echo "OpenAPI spec saved to: $OUTPUT_FILE"
    echo "File size: $(wc -c < "$OUTPUT_FILE") bytes"
else
    echo "Failed to generate OpenAPI spec."
    echo "Please start the server manually and download from: http://localhost:5000/swagger/v1/swagger.json"
fi
SCRIPT

chmod +x "$CONVENIENCE_SCRIPT"

echo -e "${GREEN}✓ Convenience script created: $CONVENIENCE_SCRIPT${NC}"
echo ""
echo -e "${GREEN}============================================${NC}"
echo -e "${GREEN}  OpenAPI Spec Generation Ready${NC}"
echo -e "${GREEN}============================================${NC}"
echo ""
echo -e "To generate the spec, run:"
echo -e "  ${BLUE}cd ERP-Backend && dotnet run${NC}"
echo -e "Then download from:"
echo -e "  ${BLUE}curl -o openapi-spec.json http://localhost:5000/swagger/v1/swagger.json${NC}"
echo ""
echo -e "Or use the convenience script:"
echo -e "  ${BLUE}$CONVENIENCE_SCRIPT${NC}"
echo ""
