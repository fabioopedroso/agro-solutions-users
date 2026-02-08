#!/bin/bash

echo "=========================================="
echo "  Deploying AgroSolutions to Minikube"
echo "=========================================="
echo ""

# Get the current directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "📦 Step 1: Creating namespace..."
kubectl apply -f "$SCRIPT_DIR/namespace.yaml"
echo ""

echo "🔐 Step 2: Creating secrets..."
kubectl apply -f "$SCRIPT_DIR/infra/secrets.yaml"
echo ""

echo "🐰 Step 3: Deploying RabbitMQ..."
kubectl apply -f "$SCRIPT_DIR/infra/rabbitmq-deployment.yaml"
kubectl apply -f "$SCRIPT_DIR/infra/rabbitmq-service.yaml"
echo ""

echo "⏳ Waiting for RabbitMQ to be ready..."
kubectl wait --for=condition=ready pod -l app=rabbitmq -n agrosolutions --timeout=120s
echo "✅ RabbitMQ is ready!"
echo ""

echo "🚀 Step 4: Deploying microservices..."

# Users API
echo "  - Deploying Users API..."
kubectl apply -f "$SCRIPT_DIR/configmap.yaml"
kubectl apply -f "$SCRIPT_DIR/deployment.yaml"
kubectl apply -f "$SCRIPT_DIR/service.yaml"

# Properties API
echo "  - Deploying Properties API..."
kubectl apply -f "$SCRIPT_DIR/../../agro-solutions-properties-fields/k8s/"

# Sensor Ingestion
echo "  - Deploying Sensor Ingestion..."
kubectl apply -f "$SCRIPT_DIR/../../agro-solutions-sensor-ingestion/k8s/"

# Analysis Alerts Worker
echo "  - Deploying Analysis Alerts Worker..."
kubectl apply -f "$SCRIPT_DIR/../../agro-solutions-analysis-alerts/k8s/"

# Sensor Simulator
echo "  - Deploying Sensor Simulator..."
kubectl apply -f "$SCRIPT_DIR/../../agro-solutions-sensor-simulator/k8s/"

echo ""
echo "✅ Deployment complete!"
echo ""
echo "=========================================="
echo "  Cluster Status"
echo "=========================================="
kubectl get pods -n agrosolutions
echo ""
echo "=========================================="
echo "  Service Endpoints"
echo "=========================================="
MINIKUBE_IP=$(minikube ip 2>/dev/null || echo "localhost")
echo "PostgreSQL: Supabase (gerenciado externamente)"
echo "RabbitMQ Management: http://$MINIKUBE_IP:30672"
echo "Users API: http://$MINIKUBE_IP:30001"
echo "Properties API: http://$MINIKUBE_IP:30002"
echo "Sensor Ingestion: http://$MINIKUBE_IP:30003"
echo ""
echo "=========================================="
echo "  Next Steps"
echo "=========================================="
echo "1. Configure Supabase connection strings in infra/secrets.yaml"
echo "2. Build and push Docker images to registry"
echo "3. Update deployment.yaml files with correct image names"
echo "4. Run migrations to Supabase"
echo "5. Create a user and get JWT token"
echo "6. Update sensor-simulator deployment with JWT token"
echo ""
