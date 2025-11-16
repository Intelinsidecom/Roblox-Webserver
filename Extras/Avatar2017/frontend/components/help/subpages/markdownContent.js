import { createUseStyles } from "react-jss";
import ReactMarkdown from "react-markdown";
import remarkGfm from 'remark-gfm'
import { useEffect, useState } from "react";
import { getMarkdownContent } from "../../../services/util";
import Link from "../../link";
import rehypeRaw from "rehype-raw";

const useStyles = createUseStyles({
    div: {
        '& strong': {
            fontWeight: '600!important'
        },
        '& table': {
            borderSpacing: 0,
            borderCollapse: 'collapse',
            display: 'block',
            marginTop: 0,
            marginBottom: '16px',
            width: 'max-content',
            maxWidth: '100%',
            overflow: 'auto',
        },
        '& tr': {
            backgroundColor: '#fff',
            borderTop: '1px solid hsla(210, 18%, 87%, 1)',
        },
        '& tr:nth-child(2n)': {
            backgroundColor: '#f6f8fa',
        },
        '& td, & th': {
            padding: '6px 13px',
            border: '1px solid #d0d7de',
        },
        '& th': {
            fontWeight: 700,
        },
        '& table img': {
            backgroundColor: 'transparent'
        }
    },
});

/**
 * 
 * @param {{mdUrl: string;}} props 
 * @returns 
 */
const MarkdownContent = props => {
    const s = useStyles();
    const [markdown, setMarkdown] = useState(``);

    useEffect(() => {
        getMarkdownContent(props.mdUrl).then(md => {
            setMarkdown(md);
        });
    }, [props])

    const components = {
        a: ({ href, children, ...props }) => (
            <Link href={href}>
                <a {...props} className={`link2018`}>{children}</a>
            </Link>
        ),
        img: ({ src, alt, ...props }) => (
            <img src={src || ""} alt={alt || ""} style={{ maxWidth: '100%', margin: '6px 0' }} {...props} />
        )
    };

    return <div className={s.div}>
        <ReactMarkdown
            // @ts-ignore
            components={components}
            // rehypePlugins={[rehypeRaw]}
            remarkPlugins={[remarkGfm]}
            // remarkRehypeOptions={{ passThrough: ['link'] }}
        >{markdown}</ReactMarkdown>
    </div>
}

export default MarkdownContent;