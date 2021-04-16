using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

/*
    Authenticators: https://mirror-networking.com/docs/Components/Authenticators/
    Documentation: https://mirror-networking.com/docs/Guides/Authentication.html
    API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

public class MMONetworkAuthenticator : NetworkAuthenticator
{
    [Header("Components")] 
    public MMONetworkManager networkManager;

    [Header("Login")] 
    public string loginUsername;
    public string loginPassword;

    [Header("Security")] 
    public string passwordSalt = "7NCxS@iTFVeoCDKr*HXFV#^b9";
    public int usernameMaxLength = 16;

    #region Server

    /// <summary>
    /// Called on server from StartServer to initialize the Authenticator
    /// <para>Server message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartServer()
    {
        // Register login message, allowed if not authenticated
        NetworkServer.RegisterHandler<LoginMsg>(OnServerLogin, false);
    }

    /// <summary>
    /// Called on server from OnServerAuthenticateInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    public override void OnServerAuthenticate(NetworkConnection conn)
    {
        // wait for LoginMsg from client
    }

    /// <summary>
    /// Checks entered username
    /// </summary>
    /// <param name="username">Player's username.</param>
    public bool IsAllowedUsername(string username)
    {
        // Only contains letters, number and underscore and not empty
        return username.Length <= usernameMaxLength &&
               Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$");
    }
    
    /// <summary>
    /// Checks entered password
    /// </summary>
    /// <param name="password">Player's password.</param>
    public bool IsAllowedPassword(string password)
    {
        // password should never be empty
        return password.Length > 0;
    }
    
    /// <summary>
    /// Returns true if is in Lobby and false if is online
    /// </summary>
    /// <param name="username">Player's username.</param>
    bool AccountLoggedIn(string username)
    {
        // In lobby (creating character) or in world?
        return networkManager.lobby.ContainsValue(username) ||
               Player.onlinePlayers.Values.Any(p => p.username == username);
    }
    
    /// <summary>
    /// When server receives LoginMsg from the client
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    /// <param name="message">LoginMsg sent from client.</param>
    void OnServerLogin(NetworkConnection conn, LoginMsg message)
    {
        // Correct version? - Avoids outdated client/server combination
        if (message.version == Application.version)
        {
            // Allowed username? password not empty?
            if (IsAllowedUsername(message.username) && IsAllowedPassword(message.password))
            {
                // Validate account info
                if (DatabaseManager.singleton.Login(message.username, message.password))
                {
                    // Not in lobby and not in world?
                    if (!AccountLoggedIn(message.username))
                    {
                        // Add to logged in accounts array
                        networkManager.lobby[conn] = message.username;

                        // Login successful
                        print("Login successful: " + message.username);

                        // Notify client about successful login.
                        conn.Send(new LoginSuccessMsg());

                        // Authenticate on server
                        OnServerAuthenticated.Invoke(conn);
                    }
                    else
                    {
                        print("Account already logged in: " + message.username);
                        networkManager.ServerSendError(conn, "Already logged in!", false);
                    }
                }
                else
                {
                    print("Invalid username or password for: " + message.username);
                    networkManager.ServerSendError(conn, "Invalid username or password!", false);
                }
            }
            else
            {
                print("Username or password empty. Network state: " + networkManager.state.ToString());
                networkManager.ServerSendError(conn, "Username or password empty!", false);
            }
        }
        else
        {
            print("Version mismatch: " + message.username + " expected:" + Application.version + " received: " + message.version);
            networkManager.ServerSendError(conn, "Please update your client.", false);
        }
    }

    #endregion

    #region Client

    /// <summary>
    /// Called on client from StartClient to initialize the Authenticator
    /// <para>Client message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartClient()
    {
        // Register login success message, allowed before authenticated
        NetworkClient.RegisterHandler<LoginSuccessMsg>(OnClientLoginSuccess, false);
    }
    
    /// <summary>
    /// PBKDF2 hashing recommended by NIST:
    /// http://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-132.pdf
    /// Salt should be at least 128 bits = 16 bytes
    /// </summary>
    public static string PBKDF2Hash(string text, string salt)
    {
        byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
        Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(text, saltBytes, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

    /// <summary>
    /// Called on client from OnClientAuthenticateInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection of the client.</param>
    public override void OnClientAuthenticate(NetworkConnection conn)
    {
        // Send login packet with hashed password, so that the original password
        // is not broadcast over the network.
        //
        // Application.version is modified under:
        // Edit -> Project Settings -> Player -> Bundle Version
        string hash = PBKDF2Hash(loginPassword, passwordSalt + loginUsername);
        LoginMsg message = new LoginMsg
        {
            username = loginUsername, 
            password = hash, 
            version = Application.version
        };
        conn.Send(message);
        print("login message was sent");

        // set state
        networkManager.state = NetworkState.Handshake;
    }
    
    /// <summary>
    /// Request OnClientConnected to be called
    /// </summary>
    void OnClientLoginSuccess(NetworkConnection conn, LoginSuccessMsg msg)
    {
        // Authenticated successfully. OnClientConnected will be called.
        OnClientAuthenticated.Invoke(conn);
    }

    #endregion
}
