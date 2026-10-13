FROM node:22-bookworm-slim
WORKDIR /app
COPY package*.json ./
RUN npm ci --omit=dev
COPY . .
RUN mkdir -p /var/lib/zip-slip/temp_extract && useradd -u 1001 -m appuser && chown -R appuser:appuser /app /var/lib/zip-slip
USER appuser
EXPOSE 3000
CMD ["npm", "start"]
