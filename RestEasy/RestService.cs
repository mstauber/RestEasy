﻿using RestEasy.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RestEasy
{

public delegate void RestRequestHandler(RestRequest request, RestResponse response);
public delegate void RestErrorHandler(RestService restService, Exception eventArgs);

public class RestService
{

    public event RestErrorHandler Error;

	public RestService()
	{
		IsListening = false;
		m_requestTree = new RestRequestTree();
	}


    public void Register(RestMethod method, string uri, RestRequestHandler handler)
    {
        //check if running
		if (IsListening)
			throw new ApplicationException("RestService is listening, cannot register new methods");

        m_requestTree.AddRequestHandler(uri, method, handler);
    }


    public void Listen(int port)
    {
        if (IsListening)
            throw new ApplicationException("RestService is already listening");

        IsListening = true;

        m_httpServer = new RestHttpServer(port);

        m_httpServer.Error += OnServerError;
        m_httpServer.Request += OnHttpRequest;
    
		m_httpServer.Listen();
    }

	public bool IsListening { get; private set; }

    private void OnHttpRequest(HttpListenerRequest httpRequest, HttpListenerResponse httpResponse)
    {
		var url = httpRequest.RawUrl;

        var method = HttpMethodToRestMethod(httpRequest.HttpMethod);

        RestRequestParameters parameters;

        var handler = m_requestTree.GetRequestHandler(url, method, out parameters);


        handler(new RestRequest(httpRequest, parameters), new RestResponse(httpResponse));
    }

    private void OnServerError(RestHttpServer restHttpServer, Exception exception)
    {
        if(Error != null)
		    Error(this, exception);
    }


    private RestMethod HttpMethodToRestMethod(string method)
    {
        Console.WriteLine(method);
        switch (method)
        {
            case "GET":
                return RestMethod.GET;
            case "POST":
                return RestMethod.POST;
            default:
                throw new Exception("Bad http method");
        }
    }


	private RestHttpServer m_httpServer;
	private RestRequestTree m_requestTree;

}
}
