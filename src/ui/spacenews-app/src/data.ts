export type NewsEntry = {
    title: string;
    description: string;
    source: string;
    publishDate: string;
    links: {
        title: string,
        uri: string
    }[]
};

export type Source = {
    id: string;
    name: string;
    isSelected: boolean
}