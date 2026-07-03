import urllib.request
import urllib.error
import json

url = "https://localhost:8000/swagger/v1/swagger.json"
try:
    req = urllib.request.Request(url)
    import ssl
    ctx = ssl.create_default_context()
    ctx.check_hostname = False
    ctx.verify_mode = ssl.CERT_NONE
    
    response = urllib.request.urlopen(req, context=ctx)
    data = response.read().decode()
    if 'impersonate' in data:
        print("Found 'impersonate'")
    else:
        print("Did not find 'impersonate'")
    
    # Just to be sure, check if login is there
    if 'login' in data:
        print("Found 'login'")
    else:
        print("Did not find 'login'")
except Exception as e:
    print(f"Error: {e}")
