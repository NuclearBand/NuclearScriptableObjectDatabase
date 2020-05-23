BRANCH=$1
TAG=$2
git pull 
git subtree split --prefix=Packages/com.nuclearband.sodatabase -b $BRANCH
#git filter-branch --prune-empty --tree-filter 'rm -rf Tests' upm
git gc
git filter-repo --force --invert-paths --path Samples.meta --path-rename "Samples:Samples~" --refs $BRANCH
git tag $TAG $BRANCH
git push origin $BRANCH --tags