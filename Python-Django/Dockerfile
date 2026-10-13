FROM python:3.11-slim

WORKDIR /app
RUN pip install --no-cache-dir flask==3.0.3 jinja2==3.1.4
COPY attacks/02_file_upload_zip_slip/ /app/
RUN mkdir -p /var/lib/zip-slip/temp_extract && useradd -u 1001 -m -s /bin/bash appuser && chown -R appuser:appuser /app /var/lib/zip-slip
EXPOSE 80
USER appuser
CMD ["python", "server.py"]
