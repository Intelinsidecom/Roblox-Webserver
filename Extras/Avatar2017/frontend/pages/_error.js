import ErrorPage from "../components/errorPage";

const errorCodes = {
    400: {
        title: "Bad Request",
        desc: "There was a problem with your request",
    },
    401: {
        title: "Unauthorized",
        desc: "You must log in to complete this request",
    },
    403: {
        title: "Access Denied",
        desc: "You don't have permission to view this page",
    },
    404: {
        title: "Page cannot be found or no longer exists",
        desc: "Page Not found"
    },
    429: {
        title: "Too Many Requests",
        desc: "You have sent too many requests in a short period. Please try again later",
    },
    502: {
        title: "Bad Gateway",
        desc: "The server received an invalid response from an upstream server",
    },
    503: {
        title: "Service Unavailable",
        desc: "The server is temporarily unavailable. Please try again later",
    },
    504: {
        title: "Gateway Timeout",
        desc: "The server took too long to respond. Please try again later",
    },
};

export default function Error({ statusCode }) {
    const code = errorCodes[statusCode];
    if (code) {
        return <ErrorPage title={code.title} desc={code.desc} code={statusCode} />
    }
    
    // error 500
    return <ErrorPage />
}

Error.getInitialProps = ({ res, err }) => {
    const statusCode = res ? res.statusCode : err ? err.statusCode : 404;
    return { statusCode };
};