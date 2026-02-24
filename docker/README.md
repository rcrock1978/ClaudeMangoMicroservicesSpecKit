# Docker Configuration for Mango Microservices

This directory contains multi-stage Dockerfiles for all microservices in the Mango e-commerce platform.

## Dockerfiles

Each service has its own optimized multi-stage Dockerfile:

- `Dockerfile.auth` - Authentication service (port 7001)
- `Dockerfile.product` - Product catalog service (port 7002)
- `Dockerfile.shoppingcart` - Shopping cart service (port 7004)
- `Dockerfile.coupon` - Coupon service (port 7003)
- `Dockerfile.order` - Order management service (port 7005)
- `Dockerfile.email` - Email service (port 7006)
- `Dockerfile.reward` - Reward points service (port 7007)
- `Dockerfile.gateway` - API Gateway (port 7100)
- `Dockerfile.web` - MVC Web frontend (port 7200)

## Build Features

- **Multi-stage builds**: Separate build and runtime stages for minimal image size
- **Alpine Linux**: Uses lightweight Alpine-based .NET images
- **Non-root user**: Runs as unprivileged user for security
- **Optimized layers**: Efficient Docker layer caching

## Building Individual Services

To build a specific service:

```bash
# Build Auth service
docker build -f docker/Dockerfile.auth -t mango/auth:latest .

# Build Product service
docker build -f docker/Dockerfile.product -t mango/product:latest .

# And so on for other services...
```

## Next Steps

After creating these Dockerfiles, the next phase is to:

1. Update `docker-compose.yml` to include all microservices
2. Configure service dependencies and networking
3. Set up environment variables and configuration
4. Add health checks and proper startup ordering
5. Test the complete containerized system

## Image Optimization

Current images are optimized for:
- Minimal attack surface (Alpine + non-root user)
- Fast startup times
- Small image sizes
- Production readiness

Each image contains only the runtime dependencies needed to run the service.