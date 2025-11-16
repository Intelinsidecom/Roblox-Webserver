import request, { getBaseUrl } from "../lib/request"

export const getMarkdownContent = (filename) => {
  return request('GET', `/markdown/${filename}`).then(d => d.data)
}