import urllib.request
import urllib.error

url = "https://localhost:8000/swagger/v1/swagger.json"
try:
    req = urllib.request.Request(url)
    import ssl
    ctx = ssl.create_default_context()
    ctx.check_hostname = False
    ctx.verify_mode = ssl.CERT_NONE
    
    response = urllib.request.urlopen(req, context=ctx)
    print("Status:", response.status)
    print(response.read().decode()[:1000]) # Print first 1000 chars
except urllib.error.HTTPError as e:
    print(f"HTTP Error: {e.code}")
    print(e.read().decode())
except Exception as e:
    print(f"Error: {e}")
