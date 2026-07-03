import urllib.request
import urllib.error
import json

# 1. Login to get a token
login_url = "https://localhost:8000/api/auth/login"
login_data = json.dumps({"email": "superadmin@wigweuniversity.edu.ng", "password": "Password123!"}).encode('utf-8')

try:
    import ssl
    ctx = ssl.create_default_context()
    ctx.check_hostname = False
    ctx.verify_mode = ssl.CERT_NONE

    req = urllib.request.Request(login_url, data=login_data, headers={'Content-Type': 'application/json'}, method='POST')
    response = urllib.request.urlopen(req, context=ctx)
    res_data = json.loads(response.read().decode())
    token = res_data['data']['token']
    print("Got token")
    
    # 2. Call impersonate
    imp_url = "https://localhost:8000/api/auth/impersonate/employee/78509e56-106d-4f47-aaea-08ded113e514"
    req2 = urllib.request.Request(imp_url, method='POST')
    req2.add_header('Authorization', f'Bearer {token}')
    req2.add_header('Content-Length', '0')
    
    try:
        response2 = urllib.request.urlopen(req2, context=ctx)
        print("Impersonate Status:", response2.status)
        print(response2.read().decode())
    except urllib.error.HTTPError as e:
        print(f"Impersonate HTTP Error: {e.code}")
        print(e.read().decode())
        
except Exception as e:
    print(f"Error: {e}")
