from Crypto.Cipher import AES
from Crypto.Util.Padding import pad
import base64
import os
import hvac
import uuid
import psycopg2

def read_root_token():
    import json

    home_directory = os.path.expanduser("~")
    print("HOME:", home_directory)
    path = home_directory+"/vault/config/vault-cluster.json"

    print(path)

    # Opening JSON file
    f = open(path)

    # returns JSON object as a dictionary
    json_data = json.load(f)

    # Closing file
    f.close()

    return json_data['root_token']


def check_database_exists(db_name, host, user, password, port):
    try:
        # Connect to the default 'postgres' database or another known database
        # to query for the existence of the target database.
        conn = psycopg2.connect(
            host=host,
            database="postgres",  # Connect to a default database to check for others
            user=user,
            password=password,
            port=port
        )
        conn.autocommit = True
        cur = conn.cursor()

        # Query pg_database to check if the database exists
        cur.execute(
            f"SELECT EXISTS(SELECT datname FROM pg_catalog.pg_database WHERE lower(datname) = lower('{db_name}'));")

        database_exists = cur.fetchone()[0]

        cur.close()
        conn.close()

        return database_exists

    except psycopg2.Error as e:
        print(f"Error connecting to PostgreSQL: {e}")
        return False


def create_database(db_name, host, user, password, port):
    conn = None
    try:
        conn = psycopg2.connect(user=user, password=password, host=host, port=port)
        conn.rollback()  # Make sure we're not in a transaction
        conn.autocommit = True  # Turn on autocommit
        with conn.cursor() as curs:
            curs.execute("create database " + db_name + ";")

        conn.autocommit = False  # Turn autocommit back off again
        print("created.!\n")
    except (Exception, psycopg2.DatabaseError) as error:
        print(error)
    finally:
        if conn is not None:
            conn.close()


# Example usage:
db_name_to_check_or_create = "keycloak"
pg_host = "localhost"
pg_user = "admin"
pg_password = "newpwd"
pg_port = "5432" # standard port number

if check_database_exists(db_name_to_check_or_create, pg_host, pg_user, pg_password, pg_port):
    print(f"Database '{db_name_to_check_or_create}' exists.")
else:
    print(f"Database '{db_name_to_check_or_create}' does not exist")
    create_database(db_name_to_check_or_create,pg_host, pg_user, pg_password, pg_port)

db_name_to_check_or_create = "jobs_db"

if check_database_exists(db_name_to_check_or_create, pg_host, pg_user, pg_password, pg_port):
    print(f"Database '{db_name_to_check_or_create}' exists.")
else:
    print(f"Database '{db_name_to_check_or_create}' does not exist.")
    create_database(db_name_to_check_or_create,pg_host, pg_user, pg_password, pg_port)


# Generate a random 256-bit (32 bytes) key
secret_key = os.urandom(32)
# encode this secret key for storing safely in database
encoded_secret_key = base64.b64encode(secret_key)
encoded_secret_key_str = str(encoded_secret_key)
print("encoded_secret_key:", encoded_secret_key.decode("utf-8"))

cipher = AES.new(secret_key, AES.MODE_CBC)  # CBC mode of operation
# The initialization vector (IV) is needed for decryption
iv = cipher.iv
encoded_iv = base64.b64encode(iv)
encoded_iv_str = str(encoded_iv)
print("encoded_iv:", encoded_iv.decode("utf-8"))

# The data to be encrypted
data = b"Sensitive data that needs encryption"
# Pad the data to be a multiple of 16 bytes
padded_data = pad(data, AES.block_size)
# Encrypt the data
ciphertext = cipher.encrypt(padded_data)

print("Ciphertext: ", ciphertext)
print("Initialization Vector: ", iv)

# Returns `None` if the key doesn't exist
v_token = os.environ.get('VAULT_TOKEN')
print("v_token: ", v_token)

if v_token is None:
   v_token = read_root_token()
   print("v_token: ", v_token)

client = hvac.Client("http://127.0.0.1:8200",v_token)

# Generate a random UUID (Version 4)
new_guid = uuid.uuid4()
# Print the UUID
print("New Guid: ", new_guid)
secret_key = '0103dafb-988e-8034-8067-250f1fad1c20'
bytes_secret_key = bytes(secret_key, 'utf-8')
padded_data_secret_key = pad(bytes_secret_key, AES.block_size,'pkcs7')
ciphertext_secret_key = cipher.encrypt(padded_data_secret_key)
secret_key_out = base64.b64encode(ciphertext_secret_key).decode("utf-8")
print("SecretKey: ", secret_key_out)

# default_api_key='ZnAeqc2QpRbiKnt4tC4PKs75kXb1lA+ymNcbPuTM+J9jnonOHKNeIDtEYOrpkdH9'
default_api_key=str(new_guid)
bytes_default_api_key = bytes(default_api_key, 'utf-8')
padded_data_default_api_key = pad(bytes_default_api_key, AES.block_size,'pkcs7')
ciphertext_default_api_key = cipher.encrypt(padded_data_default_api_key)
default_api_key_out = base64.b64encode(ciphertext_default_api_key).decode("utf-8")
print("DefaultApiKey: ", default_api_key_out)

client.secrets.kv.v2.create_or_update_secret(
    mount_point='secrets',
    path='secrets/services/reference',
    secret=dict(SecretKey=secret_key,DefaultApiKey=default_api_key,
                PKey=encoded_secret_key.decode("utf-8"),IV=encoded_iv.decode("utf-8")),
)

client.secrets.kv.v2.create_or_update_secret(
    mount_point='secrets',
    path='secrets/services/account',
    secret=dict(SecretKey=secret_key,DefaultApiKey=default_api_key,
                PKey=encoded_secret_key.decode("utf-8"),IV=encoded_iv.decode("utf-8")),
)

client.secrets.kv.v2.create_or_update_secret(
    mount_point='secrets',
    path='secrets/services/company',
    secret=dict(SecretKey=secret_key,DefaultApiKey=default_api_key,
                PKey=encoded_secret_key.decode("utf-8"),IV=encoded_iv.decode("utf-8")),
)

client.secrets.kv.v2.create_or_update_secret(
    mount_point='secrets',
    path='secrets/services/vacancy',
    secret=dict(SecretKey=secret_key,DefaultApiKey=default_api_key,
                PKey=encoded_secret_key.decode("utf-8"),IV=encoded_iv.decode("utf-8")),
)



app_id='019905e7-ae34-747b-bb4e-020c68257ef9'
bytes_app_id = bytes(app_id, 'utf-8')
padded_data_app_id = pad(bytes_app_id, AES.block_size,'pkcs7')
ciphertext_app_id = cipher.encrypt(padded_data_app_id)
app_id_out = base64.b64encode(ciphertext_app_id).decode("utf-8")

with open("CryptData.txt", "w") as data_file:
    data_file.write("SecretKey: %s\n" % secret_key)
    data_file.write("DefaultApiKey: %s\n" % default_api_key)
    data_file.write("CryptPrivateKey: %s\n" % encoded_secret_key.decode("utf-8"))
    data_file.write("CryptIV: %s\n" % encoded_iv.decode("utf-8"))
    data_file.write("SecretKeyBase64: %s\n" % secret_key_out)
    data_file.write("DefaultApiKeyBase64: %s\n" % default_api_key_out)
    data_file.write("AppIdBase64: %s\n" % app_id_out)


#>>> # Convert a UUID to a 32-character hexadecimal string
#>>> uuid.uuid4().hex
#'9fe2c4e93f654fdbb24c02b15259716c'
