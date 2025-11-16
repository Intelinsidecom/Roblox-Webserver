import request from "./request";
import matter from "gray-matter";

export async function getMarkdownData(filename) {
    const url = `/markdown/${filename}`;
    const mdContent = await request('GET', url)
    const { data, content } = matter(mdContent);

    const markdown = {
        ...data,
        content,
    };

    return markdown;
}